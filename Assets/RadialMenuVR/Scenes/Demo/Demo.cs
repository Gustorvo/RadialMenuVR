using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] Transform _parent;
        [SerializeField] RadialMenu[] _menuArray;
        [SerializeField, Range(0.1f, 1.2f)] float _delay = 0.5f;
        [SerializeField] bool _allowReverseDirection = true;
        private bool _running;
        private bool RunningInEditor => Application.isEditor && Application.isPlaying;  // needed for "EnableIf" attribute drawer
        private bool _notRunning => !_running; // needed for "EnableIf" attribute drawer
        private bool _animatingRadius = false;
        private Coroutine _radiusCoroutine;

        #region Buttons
        [Button]
        public void PopulateArrayFromParent()
        {
            var allMenus = _parent.GetComponentsInChildren<RadialMenu>();
            _menuArray = allMenus;
        }

        [Button, EnableIf(EConditionOperator.And, "RunningInEditor", "_notRunning")]
        public void StartDemo()
        {
            if (!_running) StartCoroutine(StartAnimatingMenusRoutine());           
        }
        [Button, EnableIf(EConditionOperator.And, "RunningInEditor", "_running")]
        public void StopDemo()
        {
            StopAllCoroutines();
            _running = false;
            _animatingRadius = false;
        }

        [Button, EnableIf(EConditionOperator.And, "RunningInEditor", "_running")]
        public void ToggleAnimateRadius()
        {
            if (!_running) return;
            if (!_animatingRadius)
                _radiusCoroutine = StartCoroutine(AnimateRadiusRoutine());
            else if (_animatingRadius && _radiusCoroutine != null)
            {
                StopCoroutine(_radiusCoroutine);
                _animatingRadius = false;
            }
        }

        #endregion // end Buttons

        private IEnumerator StartAnimatingMenusRoutine()
        {
            _running = true;           
            int step = 1;
            while (_running)
            {
                for (int i = 0; i < _menuArray.Length; i++)
                {
                    _menuArray[i].ShiftItems(step);
                }
                bool isFirst = _menuArray[0].ChosenIndex == 0;
                bool isLast = _menuArray[0].ChosenIndex == _menuArray[0].Items.Count - 1;
                if ((isFirst || isLast) && _allowReverseDirection)
                {
                    // reverse direction                  
                    step  *= -1;
                }
                yield return new WaitForSeconds(_delay);
            }
        }

        private IEnumerator AnimateRadiusRoutine()
        {
            _animatingRadius = true;
            float a = RadialMenu.minRadius;
            float b = .3f;
            float t = 0f;
            float newRadius;
            while (_animatingRadius)
            {                
                newRadius = Mathf.Lerp(a, b, t);
                for (int i = 0; i < _menuArray.Length; i++)
                {
                    _menuArray[i].Radius = newRadius;
                    _menuArray[i].Rebuild();
                }
                if (newRadius == b)
                {
                    // reverse
                    float temp = a;
                    a = b;
                    b = temp;
                    t = 0f;
                }
                else
                t += .5f;
                yield return new WaitForSeconds(_delay*2f);
            }           
        }
    }
}
