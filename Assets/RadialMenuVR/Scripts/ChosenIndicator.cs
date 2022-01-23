using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenIndicator : AttachmentBase
    {
        private Vector3 _currentPosition, _currentScale;

        private new void Awake()
        {
            base.Awake();
        }

        internal override void SetPosition(Vector3 position) => AttachedObj.localPosition = position;
        internal override void SetScale(Vector3 scale) => AttachedObj.localScale = scale;        

        internal override void SetLocalRotation(Quaternion targetRotation)
        {
            if (_rotate) transform.localRotation = targetRotation;            
        }

        internal override void Animate()
        {
            if (_move)
            {
                if (Menu.IsActive) MoveAnimator.Animate(ref _currentPosition, TargetPosition);
                else MoveAnimator.Animate(ref _currentPosition, TargetPosition, true, true); // make critically damped system when toggling off
                SetPosition(_currentPosition);
            }
            if (_scale)
            {
                if (Menu.IsActive) ScaleAnimator.Animate(ref _currentScale, TargetScale);
                else ScaleAnimator.Animate(ref _currentScale, TargetScale, true, true); // make critically damped system when toggling off
                SetScale(_currentScale);
            }             
        }
    }
}
