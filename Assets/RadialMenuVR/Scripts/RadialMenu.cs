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
    /// </summary>
    public class RadialMenu : MonoBehaviour
    {
        [SerializeField] MenuItem _menuItemsPrefab;
        [SerializeField, OnValueChanged("OnSettingsChangedCallback")] MenuCircleType _circleType = MenuCircleType.FullCircle;        
        [field: SerializeField, OnValueChanged("OnSettingsChangedCallback"), Range(minRadius, maxRadius)] public float Radius { get; internal set; } = minRadius;
        [field: SerializeField, EnableIf("InEditMode")] public MenuRotationType RotationType { get; private set; } = MenuRotationType.RotateItems;
        [field: SerializeField] public ItemsManager Items { get; private set; }
        [field: SerializeField] public MenuAttachments Attachments { get; private set; }
        [field: SerializeField] public Transform AnchorToFollow { get; private set; }  // menu will move after this transform (usually a VR controller)
        [field: SerializeField, OnValueChanged("OnSettingsChangedCallback")] public bool RadiusAffectsScale { get; private set; } = true;
        [field: SerializeField, ReadOnly] public int ChosenIndex { get; private set; } = 0;
        [ShowNativeProperty]  int Number_Of_Items => Items.Count; 
        public float SpaceRad => _circleType == MenuCircleType.FullCircle ? Mathf.PI * 2f : Mathf.PI;
        public bool Initialized { get; private set; } = false;
        public MenuItem Chosen { get; private set; } = null;       
        public float ItemsDelataDistDeg => Items.Count > 0 ? SpaceDegrees / Items.Count : 0f; // angular distance (in degrees) between each elements in the menu    
        public bool InEditMode => Application.isEditor && !Application.isPlaying;
        public float SpaceDegrees => SpaceRad * Mathf.Rad2Deg;
        public ChosenIndicator Indicator
        {
            get
            {
                if (_indicator == null) _indicator = GetComponentInChildren<ChosenIndicator>();
                return _indicator;
            }
        }
        public MenuScaler Scaler
        {
            get
            {
                if (_scaler == null) _scaler = GetComponent<MenuScaler>();
                return _scaler;
            }
        }
        public MenuMover Mover
        {
            get
            {
                if (_mover == null) _mover = GetComponent<MenuMover>();
                return _mover;
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
        public bool IsActive
        {
            get => InEditMode ? true : _active;
            private set => _active = value;
        }
        public event Action<MenuItem> OnItemChosen; // get called when an item being chosen                                                  
        public event Action OnToggleVisibility; // get called when menu activates/deactivates
        public event Action OnMenuRebuild; // get called when any of the menu properties getchanged (radius, offset, scale etc)
        public event Action<int> OnStep; // menu rotation direction (defined by 1 step), where +1 => one step right, -1 => one step left 

        public const float minRadius = .1f;
        public const float maxRadius = 1f;
        private ChosenIndicator _indicator;
        private bool _active = false;
        private MenuScaler _scaler;
        private MenuMover _mover;
        private ChosenText _text;
        private int _prevIndex;

        private void Awake()
        {
            if (AnchorToFollow == null)
            {
                // no anchor defined, let this make this transform to be the anchor itself,
                // it will allow to drive pos & rot directrly 
                AnchorToFollow = transform;
            }
            Init();            
        }
        public void Rebuild()
        {
            Init();
            OnMenuRebuild?.Invoke();

            if (InEditMode)
            {
                // apply changes directly when in edit mode & not running
                Items.SetPositions(Items.GetInitialPositions());
                Scaler.Scale();
                Attachments.InitPositionAndScale();                
            }
        }

        public void Init()
        {
            Items.Init();
            InitChosen();
            Initialized = true;
        }
        private void InitChosen()
        {
            // if (!InEditMode) return;
            if (Initialized) return;
            ChosenIndex = 0;
            if (Items.TryGetItem(ChosenIndex, out MenuItem item))
            {
                Chosen = item;
                OnItemChosen?.Invoke(item);
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
            IsActive = !IsActive;
            OnToggleVisibility?.Invoke();
        }

        private void ResetPositionsAndScale()
        {
            IsActive = false;
            Indicator.SetPosition(Vector3.zero);
            Indicator.SetScale(Vector3.zero);
            Items.SetPositions(Vector3.zero);
            Items.SetScales(Vector3.zero);
        }

        /// <summary>
        /// shifts all items by step (-1/1)
        /// </summary>
        /// <param name="step"></param>
        public void ShiftItems(int step)
        {
            // if (!IsActive) return;
            _prevIndex = ChosenIndex;
            if (SetChosenIndex(step) == _prevIndex) step = 0;
            if (Items.TryGetItem(ChosenIndex, out MenuItem item))
            {
                Chosen = item;
                OnItemChosen?.Invoke(item);
            }
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

        /*  public void MakeItemChosen(GameObject chosenGO)
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
          }*/

        private void InvokeMenuChange(int step)
        {
            // invert the step (and hence the direction),
            // which is opposite of rotating all items in a circle
            if (RotationType == MenuRotationType.RotateAttachments)
                step *= -1;
            OnStep?.Invoke(step);
        }       

        private bool IsLast(int index) => index == Items.Count - 1;
        private bool IsFirst(int index) => index == 0;
        
        #region Buttons
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
        #endregion // end buttons

        #region Callbacks
        private void OnSettingsChangedCallback()
        {
            Rebuild();
        }
        #endregion // end callbacks
    }
}