using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    /// <summary>
    /// Makes Carousel-like menu (of items) that rotates around specified rotation axis (in this case around a tracked hand controller)
    /// </summary>
    public class RadialMenu : MonoBehaviour
    {
        [SerializeField] Transform _menuItemsParent;
        [SerializeField] Transform _carouselAnchor;
        [SerializeField, Range(.01f, .5f)] float _menuRadius = 0.085f;
     
        public event Action<MenuItem> OnItemChosen; // fist choose
        public event Action<MenuItem> OnItemSelected; // then select
        public event Action<int> OnRotated; // direction (step) of chosen relative to the prev one: 1/-1 
        public event Action OnToggleVisibility;
        public Transform Anchor => _carouselAnchor;
        public float Radius => _menuRadius;
        public float DistToNextItemDeg => ItemList.Count > 0 ? 360f / ItemList.Count : 0f; // distance (in degrees) between 1st and 2nd elements in carousel    
        public float CurrentRadius => Vector3.Distance(ItemList[0].Icon.transform.position, transform.position); // current distance (in m) between 1st elements and center of the radial menu
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

        public int _chosen;
        private int _capacity => ItemsPositions.Count - 1;
        private int _prevIndex;
        public List<Vector3> ItemsPositions { get; private set; } = new List<Vector3>();
        public bool Active { get; private set; } = false;

        private void Awake()
        {
            Active = false;
            // populate list of menu items                
            for (int i = 0; i < _menuItemsParent.childCount; i++)
            {
                var child = _menuItemsParent.GetChild(i);
                if (child.TryGetComponent(out MenuItem newItem) && !ItemList.Contains(newItem))
                {
                    ItemList.Add(newItem);
                    newItem.Icon.transform.parent = transform; // move icon's GO to the carousel's GO
                }
            }
            RebuildCircle();
            ResetItemsPositions();          
        }
        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            ToogleVisibility();            
        }

        public void ToogleVisibility()
        {           
            Active = !Active;         
            OnToggleVisibility?.Invoke();           
        }       

        private void ResetItemsPositions() // start "minimized"
        {
            for (int i = 0; i < ItemList.Count; i++)
            {
                ItemList[i].Icon.transform.localPosition = Vector3.zero;
            }
        }

        private void RebuildCircle()
        {
            if (ItemList.Count >= 2)
            {
                List<Vector3> itemsPositions = new List<Vector3>();

                for (int i = 0; i < ItemList.Count; i++)
                {
                    ItemsPositions.Add(GetPosition(i));
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


    }
}