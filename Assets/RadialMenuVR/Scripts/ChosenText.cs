using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenText : Attachment
    {
        [SerializeField] Transform _textTransform;
        [SerializeField] bool _textRotatesWithIndicator;
        [SerializeField] bool _textScalesWithRadius;
        private TextMesh _text;
        private Vector3 _initialPosition, _initialScale;
        private Vector3 _currentPosition, _currentScale;
        private float _distToOuterCircle;
        private Vector3 _targetPosition
        {
            get
            {
                Vector3 pos = Menu.Active ? _initialPosition : Vector3.zero;
                if (_textScalesWithRadius)
                    pos = pos +  _dirToCenter * (_distToOuterCircle);
                return pos;
            }
        }
        private Vector3 _targetScale => Menu.Active ? _initialScale : Vector3.zero;
        private Vector3 _dirToCenter => Vector3.Normalize(Vector3.zero - _textTransform.localPosition);


        private new void Awake()
        {
            base.Awake();
            Menu.OnItemChosen -= SetItemText;
            Menu.OnItemChosen += SetItemText;

            _text = GetComponentInChildren<TextMesh>();
            _initialPosition = _textTransform.localPosition;
            _initialScale = _textTransform.localScale;
            _distToOuterCircle = Vector3.Distance(Vector3.zero, _textTransform.localPosition) - Menu.Radius;
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
                MoveAnimator.Animate(ref _currentPosition, _targetPosition);
                _textTransform.localPosition = _currentPosition;
            }
            if (_scale)
            {
                ScaleAnimator.Animate(ref _currentScale, _targetScale);
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
