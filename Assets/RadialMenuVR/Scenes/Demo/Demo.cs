using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] RadialMenu[] _menuArray;
        [SerializeField, Range(0.1f, 2f)] float _delay = 1.25f;
        [SerializeField] bool _allowReverseDirection = true;
        [SerializeField] Transform _parent;
        private bool _demoRunning;
        private bool _inPlayMode => Application.isEditor && Application.isPlaying;
        private Coroutine _toggleCoroutine;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(2f);
            StartDemo();
            ToggleVisibility();
        }

        private IEnumerator StartAnimatingMenusRoutine()
        {
            _demoRunning = true;           
            int step = 1;
            while (_demoRunning)
            {
                for (int i = 0; i < _menuArray.Length; i++)
                {
                    _menuArray[i].ShiftItems(step);
                    _menuArray[i].SetSelected(true) ;                    
                }
                bool isFirst = _menuArray[0].HoveredIndex == 0;
                bool isLast = _menuArray[0].HoveredIndex == _menuArray[0].Items.Count - 1;
                if ((isFirst || isLast) && _allowReverseDirection)
                {
                    // reverse direction                  
                    step  *= -1;
                }
                yield return new WaitForSeconds(_delay);
            }
        }

        private IEnumerator ToggleVisibilityRoutine()
        {
            _demoRunning = true;         
            while (_demoRunning)
            {
                for (int i = 0; i < _menuArray.Length; i++)
                {
                    yield return new WaitForSeconds(_delay);               
                    _menuArray[i].ToogleVisibility();
                    yield return new WaitForSeconds(_delay);               
                    _menuArray[i].ToogleVisibility();
                }
            }
        }

        #region Buttons
        private bool _demoNotRunning => !_demoRunning; // needed for "EnableIf" attribute drawer 
        [Button, DisableIf("_inPlayMode")]
        public void PopulateArrayFromParent()
        {
            var allMenus = _parent.GetComponentsInChildren<RadialMenu>();
            _menuArray = allMenus;
        }

        [Button, EnableIf(EConditionOperator.And, "_inPlayMode", "_demoNotRunning")]
        public void StartDemo()
        {
            if (!_demoRunning) StartCoroutine(StartAnimatingMenusRoutine());           
        }
       
        [Button, EnableIf(EConditionOperator.And, "_inPlayMode", "_demoNotRunning")]
        public void StopDemo()
        {
            StopAllCoroutines();
            _demoRunning = false;         
            _toggleCoroutine = null;           
        }      

        [Button, EnableIf(EConditionOperator.And, "_inPlayMode", "_demoNotRunning")]
        public void ToggleVisibility()
        {
            if (_toggleCoroutine == null) // start only once
            _toggleCoroutine = StartCoroutine(ToggleVisibilityRoutine());
        }

        #endregion // end Buttons
    }
}
