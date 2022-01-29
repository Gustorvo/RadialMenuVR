using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ItemsManager : MonoBehaviour
    {
        [SerializeField] MenuItem _menuItemsPrefab;
        [SerializeField, OnValueChanged("OnColorChangedCallback")] Color _selectedColor;
        [SerializeField, OnValueChanged("OnColorChangedCallback")] Color _normalColor;
        public List<MenuItem> ItemList { get; private set; } = new List<MenuItem>();
        private Vector3[] InitialPositions { get; set; }
        private Vector3[] InitialScales { get; set; }
        public int Count
        {
            get
            {
               // if (ItemList.Count == 0) Init();
                return ItemList.Count;
            }
        }
        public float DeltaDistance => Vector3.Distance(InitialPositions[0], InitialPositions[1]);
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponentInParent<RadialMenu>();
                return _menu;
            }
        }
        private RadialMenu _menu;
        private int _childCountFromLastInit; // keeps track of number of children/items

        private void Awake()
        {
            Menu.OnItemSelected -= SetSelectedColorForItem;
            Menu.OnItemSelected += SetSelectedColorForItem;
        }

        private void OnDestroy()
        {
            Menu.OnItemSelected -= SetSelectedColorForItem;
        }
        public bool TryRemoveItem(MenuItem itemToRemove, bool destroyGO = false)
        {
            // if (itemToRemove == null) itemToRemove = ItemList.LastOrDefault();
            itemToRemove ??= ItemList.LastOrDefault();
            if (ItemList.Contains(itemToRemove))
            {
                ItemList.Remove(itemToRemove);
                if (destroyGO)
                {
                    DestroyImmediate(itemToRemove.gameObject);
                }
                return true;
            }
            return false;
        }
        public void DestroyAll()
        {
            if (!Menu.Initialized) Menu.Init();
            for (int i = 0; i < ItemList.Count; i++)
            {
                DestroyImmediate(ItemList[i].gameObject);
            }
            ItemList.Clear();
        }

        #region Init
        public void Init()
        {
            CreateItemListFromChildren();
            InitPositions(); // too costy! Optimize!            
            InitScales();
        }

        internal void CreateFromArray(IPlaceble[] placeableItems)
        {       
            DestroyAll();
            for (int i = 0; i < placeableItems.Length; i++)
            {
                MenuItem newItem = Instantiate(_menuItemsPrefab, transform);
                newItem.InitFromPlaceable(placeableItems[i]);
                ItemList.Add(newItem);
            }
            _childCountFromLastInit = transform.childCount;
            Debug.Log($"Created new menu of {placeableItems.Length} items");
            Menu.Rebuild();
        }
        public void CreateItemListFromChildren()
        {
            if (transform.childCount == _childCountFromLastInit) return; // for performance reasons, don't iterate if hierarchy hasn't changed
            ItemList = new List<MenuItem>();
            _childCountFromLastInit = transform.childCount;
            Debug.Log("Iterating over children/menu items");
            for (int i = 0; i < _childCountFromLastInit; i++)
            {
                var child = transform.GetChild(i);
                if (child.TryGetComponent(out MenuItem newItem) && !ItemList.Contains(newItem))
                {
                    newItem.Init(i, i.ToString(), null);
                    ItemList.Add(newItem);
                }
            }
        }
        public void InitPositions()
        {
            InitialPositions = new Vector3[Count];
            if (Count >= 2)
            {
                for (int i = 0; i < Count; i++)
                {
                    InitialPositions[i] = GetPosition(i);
                }
                Vector3 GetPosition(int i)
                {
                    float ratio = i / (float)Count;
                    float angRad = ratio * Menu.SpaceRad;
                    float a = Mathf.Sin(angRad);
                    float b = Mathf.Cos(angRad);
                    return new Vector3(a, b, 0f) * Menu.Radius;
                }
            }
        }
        public void InitScales()
        {
            Menu.Scaler.InitScale();
            InitialScales = Enumerable.Repeat(Menu.Scaler.ItemsInitialScale, Count).ToArray();
        }
        #endregion // end init

        #region Getters
        public bool TryGetItem(int index, out MenuItem item)
        {
            item = null;
            if (index >= 0 && index <= ItemList.Count - 1)
                item = ItemList[index];
            return item != null;
        }
        public Vector3[] GetTargetPositions()
        {
            if (Menu.IsActive)
                return (Vector3[])InitialPositions.Clone(); // to enable
            return Enumerable.Repeat(Vector3.zero, Count).ToArray(); // to disable
        }
        public Vector3[] GetTargetScales()
        {
            if (Menu.IsActive)
                return (Vector3[])InitialScales.Clone(); // to enable
            return Enumerable.Repeat(Vector3.zero, Count).ToArray(); // to disable
        }
        public Vector3[] GetInitialPositions() => (Vector3[])InitialPositions.Clone();
        public Vector3[] GetInitialScales() => (Vector3[])InitialScales.Clone();
        public Vector3[] GetPositions() => ItemList.ConvertAll(i => i.PositionLocal).ToArray();
        #endregion // end getters

        #region Setters
        public void SetPositions(Vector3 position) => ItemList.ForEach(i => i.transform.localPosition = position);
        public void SetPositions(Vector3[] positions) => ItemList.ForEach(i => i.transform.localPosition = positions[i.Index]);
        public void SetLocalRotation(Quaternion rotation)
        {
            if (Menu.RotationType == MenuRotationType.RotateItems)
                transform.localRotation = rotation;
        }
        public void SetForwardForItem(MenuItem item) => item.transform.forward = Menu.AnchorToFollow.forward;
        public void SetScales(Vector3 scale) => ItemList.ForEach(i => i.transform.localScale = scale);
        public void SetScales(Vector3[] scales) => ItemList.ForEach(i => i.transform.localScale = scales[i.Index]);
        public void SetScale(MenuItem item, Vector3 scale) => item.transform.localScale = scale;
        public void SetSelectedColorForItem(MenuItem item, bool confirmed)
        {
            if (confirmed)
            {
                SetNormalColor();
                item?.SetColor(_selectedColor);
            }
        }
        public void SetNormalColor() => ItemList.ForEach(i => i.SetColor(_normalColor));

        #endregion // end setters

        #region Editor
        // --- Buttons --- //
        [Button(enabledMode: EButtonEnableMode.Editor)]
        private void CreateItem()
        {
            Instantiate(_menuItemsPrefab, transform);
            Menu.Rebuild();
        }
        [Button(enabledMode: EButtonEnableMode.Editor)]
        private void DestroyItem()
        {
            if (!Menu.Initialized) Menu.Init();
            if (Count > 2)
            {
                if (TryRemoveItem(null, true))
                    Menu.Rebuild();
            }
            else Debug.LogWarning("Can't remove since there should be at least 2 items in the menu (for it to work)!");
        }
        // --- --- //       

        public void OnColorChangedCallback()
        {
            if (!Menu.Initialized) Menu.Init();
            SetSelectedColorForItem(Menu.SelectedItem, true);
        }
        #endregion // end editor
    }
}
