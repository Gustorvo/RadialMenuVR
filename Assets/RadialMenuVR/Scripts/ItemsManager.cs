using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ItemsManager : MonoBehaviour, IMovable
    {
        public List<MenuItem> ItemList { get; private set; } = new List<MenuItem>();
        private Vector3[] InitialPositions { get; set; }
        private Vector3[] InitialScales { get; set; }
        public int Count => ItemList.Count;
        public float DeltaDistance => Vector3.Distance(InitialPositions[0], InitialPositions[1]);
        public float DistToCenter => Vector3.Distance(ItemList[0].Position, Vector3.zero);
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponentInParent<RadialMenu>();
                return _menu;
            }
        }

        private RadialMenu _menu;
        private int _childCount; // keeps track of number of children/items       

        public void Init()
        {
            CreateItemListFromChildren();
            InitPositions(); // too costy! Optimize!            
            InitScales();
        }
        public void CreateItemListFromChildren()
        {
            ItemList ??= new List<MenuItem>();
            if (transform.childCount == _childCount) return; // for performance reasons, don't iterate if hierarchy hasn't changed
            _childCount = transform.childCount;
            Debug.Log("Iterating over children/menu items");
            for (int i = 0; i < _childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.TryGetComponent(out MenuItem newItem) && !ItemList.Contains(newItem))
                {
                    newItem.Init(i, i.ToString());
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
            //InitialScales[Menu.ChosenIndex] *= Menu.Scaler.ChosenUpscaleFactor;
        }

        public bool TryGetItem(int index, out MenuItem item)
        {
            item = null;
            if (index >= 0 && index <= ItemList.Count - 1)
                item = ItemList[index];
            return item != null;
        }     
        public Vector3[] GetTargetPositions()
        {
            if (Menu.Active)
                return (Vector3[])InitialPositions.Clone(); // to enable
            return Enumerable.Repeat(Vector3.zero, Count).ToArray(); // to disable
        }
        public Vector3[] GetTargetScales()
        {
            if (Menu.Active)
                return (Vector3[])InitialScales.Clone(); // to enable
            return Enumerable.Repeat(Vector3.zero, Count).ToArray(); // to disable
        }
        public Vector3[] GetInitialPositions() => (Vector3[])InitialPositions.Clone();
        public Vector3[] GetScales() => ItemList.ConvertAll(i => i.Scale).ToArray();
        public Vector3[] GetPositions() => ItemList.ConvertAll(i => i.Position).ToArray();
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
        public void SetForwardVector(Vector3 forward) => ItemList.ForEach(i => i.transform.forward = forward);
        public void SetPositions(Vector3 position) => ItemList.ForEach(i => i.transform.localPosition = position);
        public void SetPositions(Vector3[] positions) => ItemList.ForEach(i => i.transform.localPosition = positions[i.Index]);
        public void SetRotation(Quaternion rotation) => transform.rotation = rotation;
        public void SetScales(Vector3 scale) => ItemList.ForEach(i => i.transform.localScale = scale);
        public void SetScales(Vector3[] scales) => ItemList.ForEach(i => i.transform.localScale = scales[i.Index]);
    }
}
