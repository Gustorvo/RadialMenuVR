using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenText : MonoBehaviour
    {
        [SerializeField] AnimatorSettings _moveSettings;
        [SerializeField] AnimatorSettings _scaleSettings;

        private TextMesh _text;
        private RadialMenu _menu;
        private IAnimator _moveAnimator, _scaleAnimator;
        private Vector3 _initPos, _initScale;
        private Vector3 _curPos, _curScale;
        private Vector3 _targetPos, _targetScale;

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
            Menu.OnItemChosen -= SetItemText;
            Menu.OnItemChosen += SetItemText;
            Menu.OnToggleVisibility -= ToggleVidibility;
            Menu.OnToggleVisibility += ToggleVidibility;
            _text = GetComponentInChildren<TextMesh>();

            bool springMovement = _moveSettings.AnimateUsing == Easing.NumericSpring;
            bool springScale = _scaleSettings.AnimateUsing == Easing.NumericSpring;
            _scaleAnimator = springScale ? (IAnimator)new NumericSpring(_scaleSettings) : (IAnimator)new AnimCurveLerper(_scaleSettings);
            _moveAnimator = springMovement ? (IAnimator)new NumericSpring(_moveSettings) : (IAnimator)new AnimCurveLerper(_moveSettings);
            _initPos = _text.transform.localPosition;
            _initScale = _text.transform.localScale;
        }

        private void SetItemText(MenuItem item)
        {
            _text.text = item.Text;
        }

        private void Update()
        {
            transform.forward = Menu.AnchorToFollow.forward;
        }

        public void ToggleVidibility()
        {
            if (Menu.Active)
            {
                _targetPos = _initPos;
                _targetScale = _initScale;
            }
            else
            {
                _targetPos = Vector3.zero;
                _targetScale = Vector3.zero;

            }
            StopAllCoroutines();
            StartCoroutine(ToggleVisibilityRoutine());
        }

        private IEnumerator ToggleVisibilityRoutine()
        {
            bool active = true;
            while (active)
            {
                _scaleAnimator.Animate(ref _curScale, _targetScale);
                _moveAnimator.Animate(ref _curPos, _targetPos);
                _text.transform.localPosition = _curPos;
                _text.transform.localScale = _curScale;
                yield return null;
                active = _moveAnimator.Active || _scaleAnimator.Active;
            }
        }

        private void OnDestroy()
        {
            Menu.OnItemChosen -= SetItemText;
            Menu.OnToggleVisibility -= ToggleVidibility;
        }
    }
}
