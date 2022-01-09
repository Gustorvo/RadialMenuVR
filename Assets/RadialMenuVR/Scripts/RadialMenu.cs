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
        [SerializeField] Transform _parent;
        [SerializeField] MenuItem _menuItemsPrefab;
        [SerializeField] Transform _menuAnchor; // menu will move after the anchor
        [SerializeField, OnValueChanged("OnRadiusChangedCallback")] bool _radiusChangesScale = true;
        [SerializeField, OnValueChanged("OnRadiusChangedCallback"), Range(0.1f, 2f)] float _menuRadius = 0.085f;


        public event Action<MenuItem> OnItemChosen; // fist choose
        public event Action<MenuItem> OnItemSelected; // then select
        public event Action<int> OnRotated; // direction (step) of chosen relative to the prev one: 1/-1 
        public event Action OnToggleVisibility;
        public event Action OnMenuRebuild; // get called when the menu radius has changes, or/and item(s) scale(s) changed
        public Transform Anchor => _menuAnchor;
        public Transform Parent => _parent;
        public bool RadiusChangesScale => _radiusChangesScale;
        public float Radius => _menuRadius;
        public float DistToNextItemDeg => ItemList.Count > 0 ? 360f / ItemList.Count : 0f; // angular distance (in degrees) between each elements in the menu    

        public List<MenuItem> ItemList { get; private set; } = new List<MenuItem>();
        /// <summary>
        /// index of currently chosen item in menu
        /// </summary>
        public int ChosenIndex
        {
            get => _chosen;
            private set
            { // circular loop => when max is reached, start from the beginning
                if (value == 0) return; // value hansn't changed (the same as previous)
                int nextChosen = _chosen + value;
                if (nextChosen > _capacity) _chosen = 0;
                else if (nextChosen < 0) _chosen = _capacity;
                else _chosen = nextChosen;
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

        public List<Vector3> ItemsInitialPositions { get; private set; } = new List<Vector3>();
        public bool Active { get; private set; } = false;

        private int _chosen;
        private int _capacity => ItemsInitialPositions.Count - 1;
       
        public Quaternion Rotation
        {
            get
            {
                if (_menuAnchor != null)
                {
                    return _menuAnchor.rotation;

                }
                return _parent.rotation;
            }
            set
            {
                _parent.rotation = value;
            }
        }

        internal void SetPosition(Vector3? position = null)
        {
            if (position == null)
                position = Anchor.position;
            Parent.position = Anchor.position;
        }

        private int _prevIndex;
        private MenuScaler _scaler;
        private void Awake()
        {
            if (_menuAnchor == null)
            {
                _menuAnchor = transform;
            }
            Init();
        }

        public void Init()
        {
            CreateItemListFromChildren();
            ItemsInitialPositions = GetItemsPositions();
            Scaler.CalculateScale();
        }

        private void CreateItemListFromChildren()
        {
            ItemList.Clear();
            for (int i = 0; i < _parent.transform.childCount; i++)
            {
                var child = _parent.transform.GetChild(i);
                if (child.TryGetComponent(out MenuItem newItem) && !ItemList.Contains(newItem))
                {
                    newItem.Id = i;
                    ItemList.Add(newItem);
                }
            }
        }

        internal void RemoveItem(MenuItem itemToRemove, bool destroyGO = false)
        {
            if (ItemList.Contains(itemToRemove))
            {
                ItemList.Remove(itemToRemove);
                if (destroyGO)
                {
                    DestroyImmediate(itemToRemove.gameObject);
                }
                Rebuild();
            }
        }
        [Button(enabledMode: EButtonEnableMode.Editor)]
        private void CreateItem()
        {
            Instantiate(_menuItemsPrefab, _parent.transform);
            Rebuild();
        }
        [Button(enabledMode: EButtonEnableMode.Editor)]
        private void DestroyItem()
        {
            if (ItemList.Count > 2)
            {
                RemoveItem(ItemList.LastOrDefault(), true);
            }
            else Debug.Log("Can't remove since there should be at least 2 items in the menu (for it to work)!");

        }

        private void Rebuild()
        {
            Init();
            OnMenuRebuild?.Invoke();
            bool isRunningInEditor = Application.isEditor && !Application.isPlaying;

            if (isRunningInEditor)
            {
                for (int i = 0; i < ItemList.Count; i++)
                {
                    ItemList[i].Icon.transform.localPosition = ItemsInitialPositions[i];
                }
                if (RadiusChangesScale)
                {
                    Scaler.OnScaleFactorChanged();
                }
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
            for (int i = 0; i < ItemList.Count; i++)
            {
                ItemList[i].Icon.transform.localPosition = Vector3.zero;
                ItemList[i].Icon.transform.localScale = Vector3.zero;
            }
        }

        private List<Vector3> GetItemsPositions()
        {
            List<Vector3> itemsPositions = new List<Vector3>();
            if (ItemList.Count >= 2)
            {

                for (int i = 0; i < ItemList.Count; i++)
                {
                    itemsPositions.Add(GetPosition(i));
                }
                Vector3 GetPosition(int i)
                {
                    float ratio = i / (float)ItemList.Count;
                    ratio *= 2f * Mathf.PI;
                    float a = Mathf.Sin(ratio);
                    float b = Mathf.Cos(ratio);
                    return new Vector3(a, b, 0f) * _menuRadius;
                }
            }
            return itemsPositions;
        }

        /// <summary>
        /// shifts all items by step (-1/1)
        /// </summary>
        /// <param name="step"></param>
        public void ShiftItems(int step)
        {
            if (!Active) return;
            _prevIndex = ChosenIndex;
            ChosenIndex = step;
            InvokeChosen(step);
        }


        public void MakeItemChosen(GameObject chosenGO)
        {
            if (chosenGO.TryGetComponent(out MenuItem chosenItem) && ItemList.Contains(chosenItem))
            {
                bool clockwise = false;
                int index = ItemList.IndexOf(chosenItem);
                if (index == _prevIndex) // index hasn't changed since last shift
                {
                    InvokeChosen(0);
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
                InvokeChosen(direction);

                //RotateCarousel(direction);
            }
        }

        private void InvokeChosen(int step)
        {
            OnRotated?.Invoke(step);
            OnItemChosen?.Invoke(ItemList[_chosen]);
        }

        private bool IsLast(int index) => index == ItemList.Count - 1;
        private bool IsFirst(int index) => index == 0;

        private void OnRadiusChangedCallback()
        {
            Rebuild();
        }
    }
}