using System;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class AttachedText : AttachmentBase
    {      
        private TextMesh _text;     
        private Vector3 _currentPosition, _currentScale;
        private Quaternion _currentRotation;

        private new void Awake()
        {
            base.Awake();
            Menu.OnItemHovered -= SetItemText;
            Menu.OnItemHovered += SetItemText;

            _text = GetComponentInChildren<TextMesh>();          
        }
       
        internal override void SetLocalRotation(Quaternion targetRotation)
        {
            if (_rotate) transform.localRotation = targetRotation;
            if (_alwaysLookUpRotation)
            {
                AttachedObj.forward = Menu.AnchorToFollow.forward;
            }
        }
        internal override void SetPosition(Vector3 position) => AttachedObj.localPosition = position;
        internal override void SetScale(Vector3 scale) => AttachedObj.localScale = scale;


        private void SetItemText(MenuItem item)
        {
            _text.text = item.ItemText;
        }

        internal override void Animate()
        {
            if (_move)
            {
                if (Menu.IsActive) MoveAnimator.Animate(ref _currentPosition, TargetPosition);
                else MoveAnimator.Animate(ref _currentPosition, TargetPosition, true, true); // make critically damped system when toggling off
                SetPosition(_currentPosition);
            }
            if (_rotate)
            {
                RotateAnimator.Animate(ref _currentRotation, TargetRotation);
                SetLocalRotation(_currentRotation);
            }
            if (_scale)
            {
                if (Menu.IsActive) ScaleAnimator.Animate(ref _currentScale, TargetScale);
                else ScaleAnimator.Animate(ref _currentScale, TargetScale, true, true); // make critically damped system when toggling off
                SetScale(_currentScale);
            }            
        }
        private void OnDestroy()
        {
            Menu.OnItemHovered -= SetItemText;
        }       
    }
}
