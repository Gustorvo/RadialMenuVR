using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenIndicator : Attachment
    {
        [SerializeField] Transform _iconTransform;

        private Vector3 _initialPosition, _initialScale;
        private Vector3 _currentPosition, _currentScale;

        private Vector3 _targetPosition => Menu.Active ? _initialPosition : Vector3.zero;
        private Vector3 _targetScale => Menu.Active ? _initialScale : Vector3.zero;


        private new void Awake()
        {
            base.Awake();

            Menu.OnMenuRebuild -= InitPositionAndScale;
            Menu.OnMenuRebuild += InitPositionAndScale;          

            _initialPosition = _iconTransform.localPosition;
            _initialScale = _iconTransform.localScale;
        }

        public void SetRotation(Quaternion rotation) => transform.rotation = rotation;
        public void SetPosition(Vector3 position) => _iconTransform.localPosition = position;
        public void SetScale(Vector3 scale) => _iconTransform.localScale = scale;

        public void InitPositionAndScale()
        {          
            
            Vector3 chosenPosition = Menu.transform.InverseTransformPoint(Menu.Chosen.transform.position);           
              Vector3 dirToCenter = Vector3.Normalize(Vector3.zero - chosenPosition);
            float itemScale = Menu.Active ? Menu.Scaler.UniformScale : 0f;
            Vector3 a = Vector3.zero; //chosenPosition + dirToCenter * itemScale;
            Vector3 b = chosenPosition - dirToCenter * itemScale;
            Vector3 newPos = Quaternion.Inverse(transform.localRotation) * Vector3.Lerp(a, b, Menu.IndicatorPosition);
            Vector3 newScale = Menu.Scaler.ItemsInitialScale;
            _initialPosition = newPos;
            _initialScale = newScale;
            Debug.Log("New init pos are set");

            // apply changes directly when in editor & not running
            if (Menu.InEditMode)
            {
                SetPosition(newPos);
                SetScale(newScale);
            }            
        }        

        internal override void Animate()
        {
            if (_move)
            {
                MoveAnimator.Animate(ref _currentPosition, _targetPosition);
                SetPosition(_currentPosition);
            }
            if (_scale)
            {
                ScaleAnimator.Animate(ref _currentScale, _targetScale);
                SetScale(_currentScale);
            }

            _iconTransform.forward = Menu.AnchorToFollow.forward;
        }
    }
}
