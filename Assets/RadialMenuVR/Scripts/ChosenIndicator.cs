using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenIndicator : MonoBehaviour, IMovable
    {
        [SerializeField] Transform _icon;
        [SerializeField] AnimatorSettings _moveSettings;
        [SerializeField] AnimatorSettings _scaleSettings;
       
        private IAnimator _moveAnimator, _scaleAnimator;
        private RadialMenu _menu;
        private Vector3 _initialPosition, _initialScale;
        private Vector3 _currentPosition, _currentScale;

        public Vector3 TargetPosition => Menu.Active ? _initialPosition : Vector3.zero;
        public Vector3 TargetScale => Menu.Active ? _initialScale : Vector3.zero;
        public GameObject Icon => _icon.gameObject;
        public Vector3 Position => Icon.transform.localPosition;
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponentInParent<RadialMenu>();
                return _menu;
            }
        }

        private void Awake()
        {
            Menu.OnToggleVisibility -= ToggleVisibility;
            Menu.OnToggleVisibility += ToggleVisibility;

            bool springMovement = _moveSettings.AnimateUsing == Easing.NumericSpring;
            bool springScale = _scaleSettings.AnimateUsing == Easing.NumericSpring;
            _scaleAnimator = springScale ? (IAnimator)new NumericSpring(_scaleSettings) : (IAnimator)new AnimCurveLerper(_scaleSettings);
            _moveAnimator = springMovement ? (IAnimator)new NumericSpring(_moveSettings) : (IAnimator)new AnimCurveLerper(_moveSettings);

            _initialPosition = Icon.transform.localPosition;
            _initialScale = Icon.transform.localScale;
        }
       
        public void SetRotation(Quaternion rotation) => transform.rotation = rotation;
        public void SetPositions(Vector3 position) => Icon.transform.localPosition = position;
        public void SetScales(Vector3 scale) => Icon.transform.localScale = scale;
        public void SetForwardVector(Vector3 forward) => Icon.transform.forward = forward;
        public void SetPositions(Vector3[] positions) => throw new System.NotImplementedException();
        public void SetScales(Vector3[] scales) => throw new System.NotImplementedException();

        private void ToggleVisibility()
        {
            StopAllCoroutines();
            StartCoroutine(ToggleVisibilityRoutine());
        }

        private IEnumerator ToggleVisibilityRoutine()
        {
            bool active = true;
            while (active)
            {
                _moveAnimator.Animate(ref _currentPosition, TargetPosition);
                _scaleAnimator.Animate(ref _currentScale, TargetScale);
                SetPositions(_currentPosition);
                SetScales(_currentScale);
                yield return null;
                active = _moveAnimator.Active || _scaleAnimator.Active;
            }
        }       

        private void OnDestroy()
        {
            Menu.OnToggleVisibility -= ToggleVisibility;
        }
    }
}
