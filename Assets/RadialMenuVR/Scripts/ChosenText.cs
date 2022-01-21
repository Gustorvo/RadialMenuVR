using System;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenText : Attachment
    {
        [SerializeField] Transform _textTransform;
        [SerializeField] bool _textRotatesWithIndicator = true;
        [SerializeField] bool _textScalesWithRadius = true;
        [SerializeField] bool _textMovesWithRadius = true;
        private TextMesh _text;
        private Vector3 _initialPosition, _initialScale;
        private Vector3 _currentPosition, _currentScale;
        private float _distToOuterCircle;
        private Vector3 _targetPosition
        {
            get
            {
                Vector3 pos = Menu.IsActive ? _initialPosition : Vector3.zero;
                if (Menu.IsActive && _textMovesWithRadius)
                {
                    pos = _dirToCenter * (Menu.Radius + Menu.Scaler.UniformScale + _distToOuterCircle);
                }
                return pos;
            }
        }
        private Vector3 _targetScale
        {
            get
            {
                Vector3 scale = Menu.IsActive ? _initialScale : Vector3.zero;
                if (Menu.IsActive && _textScalesWithRadius)
                {
                    scale = _initialScale + Menu.Scaler.UniformScale * Vector3.one;
                }
                return scale;
            }
        }
        private Vector3 _dirToCenter;


        private new void Awake()
        {
            base.Awake();
            Menu.OnItemChosen -= SetItemText;
            Menu.OnItemChosen += SetItemText;

            _text = GetComponentInChildren<TextMesh>();
            _initialPosition = _textTransform.localPosition;
            _initialScale = _textTransform.localScale;
            _distToOuterCircle = Vector3.Distance(Vector3.zero, _textTransform.localPosition) - Menu.Radius;
            _dirToCenter = Vector3.Normalize(_textTransform.localPosition - Vector3.zero);
        }

        private void Update()
        {
            if (Menu.RotationType == MenuRotationType.RotateIndicator && _textRotatesWithIndicator)
                transform.localRotation = Menu.Indicator.transform.localRotation * Menu.RotationOffset;
        }

        private void SetItemText(MenuItem item)
        {
            _text.text = item.Text;
        }

        internal override void Animate()
        {
            if (_move)
            {
                if (Menu.IsActive) MoveAnimator.Animate(ref _currentPosition, _targetPosition);
                else MoveAnimator.Animate(ref _currentPosition, _targetPosition, true, true); // make critically damped system when toggling off
                _textTransform.localPosition = _currentPosition;
            }
            if (_scale)
            {
                if (Menu.IsActive) ScaleAnimator.Animate(ref _currentScale, _targetScale);
                else ScaleAnimator.Animate(ref _currentScale, _targetScale, true, true); // make critically damped system when toggling off
                _textTransform.localScale = _currentScale;
            }

            _textTransform.forward = Menu.AnchorToFollow.forward;
        }
        private void OnDestroy()
        {
            Menu.OnItemChosen -= SetItemText;
        }
    }
}
