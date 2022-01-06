using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    [RequireComponent(typeof(MenuMover))]
    public class MenuScaler : MonoBehaviour
    {
        [SerializeField, Range(1.1f, 2f)] float _upscaleFactor = 1.25f;
        [SerializeField, Range(0.1f, 0.9f)] float _downscaleFactor = 0.5f;

        private RadialMenu _menu;
        private MenuMover _mover;
        private Coroutine _scaleCoroutine;
        private Vector3[] _initialScale;

        private void Awake()
        {
            _menu = GetComponent<RadialMenu>();
            _mover = GetComponent<MenuMover>();
            _menu.OnToggleVisibility -= ToggleScale;
            _menu.OnToggleVisibility += ToggleScale;
            _menu.OnRotated -= Scale;
            _menu.OnRotated += Scale;
        }
        private void Start()
        {
            _initialScale = _menu.ItemList.ConvertAll(i => i.Icon.transform.localScale).ToArray();
        }

        private void ToggleScale()
        {
            StopAllCoroutines();
            _scaleCoroutine = StartCoroutine(MinimizeMaximizeRoutine());
        }

        private void Scale(int step)
        {
            StopAllCoroutines();
            _scaleCoroutine = StartCoroutine(ScaleGraduallyRoutine());
        }

        private IEnumerator ScaleGraduallyRoutine()
        {
            Vector3[] startScales = _menu.ItemList.ConvertAll(i => i.Icon.transform.localScale).ToArray();
            float t = 0f;
            yield return null;
            while (_mover.IsMoving)
            // while (t != 1f)
            {
                t = _mover.GetInterpolator();
                for (int i = 0; i < _menu.ItemList.Count; i++)
                {
                    Vector3 newScale = Vector3.Lerp(startScales[i], GetTargetScale(i), t);
                    _menu.ItemList[i].Icon.transform.localScale = newScale;
                }
                yield return null;
            }
        }

        private IEnumerator MinimizeMaximizeRoutine()
        {
            // toggle scale (between either "0" or "normal initial")           
            float a = _menu.CurrentRadius; // start
            float b = _menu.Active ? _menu.Radius : 0.0001f; //end
            float current = 0f;
            float t = 0f;
            Vector3[] fromScale = _menu.ItemList.ConvertAll(i => i.Icon.transform.localScale).ToArray();
            while (t != 1f)
            {
                current = _menu.CurrentRadius;
                t = Mathf.InverseLerp(a, b, current);
                for (int i = 0; i < _menu.ItemList.Count; i++)
                {
                    Vector3 newScale = Vector3.Lerp(fromScale[i], GetTargetScale(i), t);
                    _menu.ItemList[i].Icon.transform.localScale = newScale;
                }
                yield return null;
            }
        }


        Vector3 GetTargetScale(int i)
        {
            if (!_menu.Active) return Vector3.zero;
            // find the middle
            int half = _menu.ItemList.Count / 2;
            // 'i' is chosen, scale it up!
            if (i == _menu.ChosenIndex)
                return _initialScale[i] * _upscaleFactor;
            // else
            // scale down each element gradually, bases on its distance to the chosen (the larger the distance, the smaller the scale)
            int delta = Mathf.Abs(i - _menu.ChosenIndex);
            if (delta > half)
            {
                delta = _menu.ItemList.Count - delta;
            }
            float decreasing = delta / 8f; // 8 is a magic number... feel free to experiment with it :)
            float scale = _downscaleFactor - decreasing;
            return _initialScale[i] * scale;
        }
    }
}
