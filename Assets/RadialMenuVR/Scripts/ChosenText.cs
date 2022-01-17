using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenText : MonoBehaviour
    {
        [SerializeField] Easing MoveUsing = Easing.NumericSpring;
        [SerializeField, ShowIf("MoveUsing", Easing.NumericSpring)] float _moveDamping = 0.5f;
        [SerializeField, ShowIf("MoveUsing", Easing.NumericSpring)] float _moveFrequency = 3f;
        [SerializeField, ShowIf("MoveUsing", Easing.AnimationCurve), CurveRange(0, 0, 1, 1)] AnimationCurve _moveAnimationCurve;
        [SerializeField, ShowIf("MoveUsing", Easing.AnimationCurve)] float _moveDuration = 0.5f;

        [SerializeField] Easing ScaleUsing = Easing.NumericSpring;
        [SerializeField, ShowIf("ScaleUsing", Easing.NumericSpring)] float _scaleDamping = 0.5f;
        [SerializeField, ShowIf("ScaleUsing", Easing.NumericSpring)] float _scaleFrequency = 3f;
        [SerializeField, ShowIf("ScaleUsing", Easing.AnimationCurve), CurveRange(0, 0, 1, 1)] AnimationCurve _scaleAnimationCurve;
        [SerializeField, ShowIf("ScaleUsing", Easing.AnimationCurve)] float _scaleDuration = 0.5f;

        private TextMesh _text;
        private RadialMenu _menu;
        private NumericSpring _moveSpring, _scaleSpring;
        private AnimCurveLerper _moveLerper, _scaleLerper;
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
            _moveSpring = new NumericSpring(_moveDamping, _moveFrequency);
            _moveLerper = new AnimCurveLerper(_moveAnimationCurve, _moveDuration);
            _scaleSpring = new NumericSpring(_scaleDamping, _scaleFrequency);
            _scaleLerper = new AnimCurveLerper(_scaleAnimationCurve, _scaleDuration);
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
            StartCoroutine(AnimatePopupRoutine());
        }

        private IEnumerator AnimatePopupRoutine()
        {
            bool active = true;
            while (active)
            {
                if (MoveUsing == Easing.NumericSpring)
                {
                    _moveSpring.Activate(ref _curPos, _targetPos);
                }
                else //move with animation curve
                {
                    _moveLerper.Activate(ref _curPos, _targetPos);
                }
                if (ScaleUsing == Easing.NumericSpring)
                {
                    _scaleSpring.Activate(ref _curScale, _targetScale);
                }
                else // scale with animation curve
                {
                    _scaleLerper.Activate(ref _curScale, _targetScale);
                }

                _text.transform.localPosition = _curPos;
                _text.transform.localScale = _curScale;
                yield return null;

                if (MoveUsing == Easing.NumericSpring)
                {
                    active = _moveSpring.Active || _scaleSpring.Active;
                }
                else
                {
                    active = _moveLerper.Active || _scaleLerper.Active;
                }
            }
        }

        private void OnDestroy()
        {
            Menu.OnItemChosen -= SetItemText;
            Menu.OnToggleVisibility -= ToggleVidibility;
        }
    }
}
