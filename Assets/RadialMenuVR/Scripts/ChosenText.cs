using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenText : Attachment
    {
        [SerializeField] Transform _textTransform;
        private TextMesh _text;
        private Vector3 _initPos, _initScale;
        private Vector3 _curPos, _curScale;
        private Vector3 _targetPos, _targetScale;


        private new void Awake()
        {
            base.Awake();
            Menu.OnItemChosen -= SetItemText;
            Menu.OnItemChosen += SetItemText;
            Menu.OnToggleVisibility -= ToggleTargets;
            Menu.OnToggleVisibility += ToggleTargets;
            _text = GetComponentInChildren<TextMesh>();

            _initPos = _textTransform.localPosition;
            _initScale = _textTransform.localScale;
        }

        private void SetItemText(MenuItem item)
        {
            _text.text = item.Text;
        }

        private void ToggleTargets()
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
        }

        internal override void Animate()
        {
            if (_move)
            {
                MoveAnimator.Animate(ref _curPos, _targetPos);
                _textTransform.localPosition = _curPos;
            }
            if (_scale)
            {
                ScaleAnimator.Animate(ref _curScale, _targetScale);
                _textTransform.localScale = _curScale;
            }

            _textTransform.forward = Menu.AnchorToFollow.forward;
        }
        private void OnDestroy()
        {
            Menu.OnItemChosen -= SetItemText;
            Menu.OnToggleVisibility -= ToggleTargets;
        }
    }
}
