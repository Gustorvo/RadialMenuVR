using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gustorvo.RadialMenu
{
    /// <summary>
    /// Makes Carousel-like menu (of items) that rotates around specified rotation axis 
    /// (in this case around a tracked hand controller)
    /// </summary>
    public class RadialMenu : MonoBehaviour
    {        
        //TODO: fix attachements
        //TODO: fix runtime menu rotation type change
        //TODO: fix snapping thresthold issue
        //TODO: fix animator velocity issue
        //TODO: fix numeric spring Vector3 overload method
        //TODO: update README

        [field: SerializeField] public ItemsManager Items { get; private set; }
        [SerializeField] private ChosenIndicator _chosenIndicator;
        // [SerializeField] Transform _items;
        [SerializeField] MenuItem _menuItemsPrefab;
        [SerializeField] Transform _menuAnchorToFollow; // menu will move after the anchor
        [SerializeField, OnValueChanged("OnSettingsChangedCallback")] bool _radiusAffectsScale = true;
        [SerializeField] bool _textRotatesWithIndicator = false;
        [SerializeField, OnValueChanged("OnSettingsChangedCallback"), Range(minRadius, maxRadius)] float _menuRadius = 0.085f;
        [SerializeField, OnValueChanged("OnSettingsChangedCallback"), Range(-180, 180), EnableIf("IsRunningInEditor")] int _rotationOffset = 0;
        [SerializeField, OnValueChanged("OnSettingsChangedCallback"), Range(0f, 1f)] private float _indicatorOffset = 0.5f;  // distance from the center to the chosen
        [SerializeField, OnValueChanged("OnSettingsChangedCallback")] MenuCircleType _circleType = MenuCircleType.FullCircle;
        [SerializeField, OnValueChanged("OnSettingsChangedCallback"), EnableIf("IsRunningInEditor")] ChosenOffset _chosenStartsOn = ChosenOffset.Beginning;
        [field: SerializeField, EnableIf("IsRunningInEditor")] public MenuRotationType RotationType { get; private set; } = MenuRotationType.RotateMenu;

        public const float minRadius = .1f;
        public const float maxRadius = 1f;

        public event Action<MenuItem> OnItemChosen; // fist choose
        public event Action<MenuItem> OnItemSelected; // then select
        public event Action<int> OnStep; // direction (step) of chosen relative to the prev one: 1/-1 
        public event Action OnToggleVisibility; // get called when menu activates/deactivates
        public event Action OnMenuRebuild; // get called when any of the menu properties has has changed (radius, offset, scale etc)
        //public MenuRotationType RotationType => _rotationType;
        public float IndicatorOffset => _indicatorOffset;
        public Transform AnchorToFollow => _menuAnchorToFollow;
        //  public ItemsManager Items => _items;
        public bool RadiusAffectsScale => _radiusAffectsScale;
        public float Radius => _menuRadius;
        public float ItemsDelataDistDeg => Items.Count > 0 ? SpaceDegrees / Items.Count : 0f; // angular distance (in degrees) between each elements in the menu    
        public float SpaceDegrees => SpaceRad * Mathf.Rad2Deg;
        public float SpaceRad => _circleType == MenuCircleType.FullCircle ? Mathf.PI * 2f : Mathf.PI;

        /// <summary>
        /// index of currently chosen item in menu
        /// </summary>
        [field: SerializeField, ReadOnly]
        public int ChosenIndex { get; private set; }
        public MenuScaler Scaler
        {
            get
            {
                if (_scaler == null) _scaler = GetComponent<MenuScaler>();
                return _scaler;
            }
        }
        public ChosenText Text
        {
            get
            {
                if (_text == null) _text = GetComponentInChildren<ChosenText>();
                return _text;
            }
        }

        public ChosenIndicator Indicator => _chosenIndicator;
        public bool Active
        {
            get => IsRunningInEditor ? true : _active;
            private set => _active = value;
        }
        public Quaternion FollowRotation => _menuAnchorToFollow.rotation; // anchor rotation    
        public Quaternion RotationOffset { get; private set; } = Quaternion.identity;
        public bool Initialized { get; private set; } = false;
        public bool IsRunningInEditor => Application.isEditor && !Application.isPlaying;
        private bool _active = false;
        private int _prevIndex;
        private MenuScaler _scaler;
        private ChosenText _text;

        private void Awake()
        {
            if (_menuAnchorToFollow == null)
            {
                _menuAnchorToFollow = transform;
            }
            Init();
        }

        public void Init()
        {
            Items.Init();
            InitChosen();
            InitRotationOffset();
            Initialized = true;
        }
        private void InitChosen()
        {
            if (!IsRunningInEditor) return;

            int chosen =
                             _chosenStartsOn == ChosenOffset.Middle ? Items.Count / 2 : // middle
                            _chosenStartsOn == ChosenOffset.End ? Items.Count - 1 :     // end
                            0;                                                           // start
            
            ChosenIndex = chosen;
        }

        private void InitRotationOffset()
        {
            RotationOffset = Quaternion.AngleAxis(_rotationOffset, Vector3.forward);
            Items.SetRotation(RotationOffset);
            Indicator.SetRotation(RotationOffset);
        }

        [Button(enabledMode: EButtonEnableMode.Editor)]
        private void CreateItem()
        {
            Instantiate(_menuItemsPrefab, Items.transform);
            Rebuild();
        }
        [Button(enabledMode: EButtonEnableMode.Editor)]
        private void DestroyItem()
        {
            if (!Initialized) Init();
            if (Items.Count > 2)
            {
                if (Items.TryRemoveItem(null, true))
                    Rebuild();
            }
            else Debug.LogWarning("Can't remove since there should be at least 2 items in the menu (for it to work)!");
        }

        public void Rebuild()
        {
            Init();
            OnMenuRebuild?.Invoke();

            if (IsRunningInEditor)
            {
                Items.SetPositions(Items.GetInitialPositions());
                Scaler.Scale();
                Indicator.InitPositionAndScale();
            }
        }

        private IEnumerator Start()
        {
            ResetPositionsAndScale(); // start "minimized"
            yield return new WaitForSecondsRealtime(0.5f); // activate
            ToogleVisibility();
        }

        public void ToogleVisibility()
        {
            Active = !Active;
            OnToggleVisibility?.Invoke();
        }

        private void ResetPositionsAndScale()
        {
            Active = false;
            Indicator.SetPositions(Vector3.zero);
            Indicator.SetScales(Vector3.zero);
            Items.SetPositions(Vector3.zero);
            Items.SetScales(Vector3.zero);
        }

        /// <summary>
        /// shifts all items by step (-1/1)
        /// </summary>
        /// <param name="step"></param>
        public void ShiftItems(int step)
        {
            if (!Active) return;
            _prevIndex = ChosenIndex;
            if (SetChosenIndex(step) == _prevIndex) step = 0;
            InvokeMenuChange(step);
        }
        private int SetChosenIndex(int step)
        {
            if (_circleType == MenuCircleType.HalfCircle)
            {
                // don't do circular loop, but stop when on last/first element
                ChosenIndex = Mathf.Clamp(ChosenIndex + step, 0, Items.Count - 1);
                return ChosenIndex;
            }

            // circular loop => when max is reached, start from the beginning (0th element)           
            int nextChosen = ChosenIndex + step;
            if (nextChosen > Items.Count - 1) ChosenIndex = 0; // shift forward - start from 0th element
            else if (nextChosen < 0) ChosenIndex = Items.Count - 1; // shift backwards - start from last element
            else ChosenIndex = nextChosen;
            return ChosenIndex;
        }

        public void MakeItemChosen(GameObject chosenGO)
        {
            if (chosenGO.TryGetComponent(out MenuItem chosenItem))
            {
                bool clockwise = false;
                int index = chosenItem.Index;
                if (index == _prevIndex) // index hasn't changed since last shift
                {
                    InvokeMenuChange(0);
                    return;
                }

                // find moving (rotational) direction
                if (IsLast(index) && IsFirst(_prevIndex))
                {
                    clockwise = false;
                }
                else if (IsLast(_prevIndex) && IsFirst(index))
                {
                    clockwise = true;
                }
                else
                {
                    clockwise = index > _prevIndex;
                }
                _prevIndex = index;
                int direction = clockwise ? 1 : -1;

                ChosenIndex = direction;
                InvokeMenuChange(direction);
            }
        }

        private void InvokeMenuChange(int step)
        {
            if (RotationType == MenuRotationType.RotateIndicator)
                // invert the step (and hence the direction),
                // which is opposite of rotating all items in a circle
                step *= -1;
            OnStep?.Invoke(step);
            if (Items.TryGetItem(ChosenIndex, out MenuItem item))
                OnItemChosen?.Invoke(item);
        }

        private bool IsLast(int index) => index == Items.Count - 1;
        private bool IsFirst(int index) => index == 0;

        #region Callbacks
        private void OnSettingsChangedCallback()
        {
            Rebuild();
        }      
        #endregion // end callbacks
    }
}