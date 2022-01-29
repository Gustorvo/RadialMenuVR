using NaughtyAttributes;
using System;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public abstract class AttachmentBase : MonoBehaviour
    {
        [field: SerializeField] public Transform AttachedObj { get; protected set; }
        [field: SerializeField] public AnimatorSettings MoveSettings { get; private set; }
        [field: SerializeField] public AnimatorSettings RotateSettings { get; private set; }
        [field: SerializeField] public AnimatorSettings ScaleSettings { get; private set; }

        [SerializeField] protected bool _move = true; // moves when toggling on/off
        [SerializeField] protected bool _scale = true; // scales when toggling on/off
        [SerializeField] protected bool _rotate = true; // rotates around the circle's center
        [SerializeField] protected bool _movesAsRadiusChanging = true;
        [SerializeField] protected bool _scalesAsRadiusChanging = true;
        [SerializeField] protected bool _alwaysLookUpRotation = true;

        protected RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponentInParent<RadialMenu>();
                return _menu;
            }
        }
        protected IAnimator MoveAnimator { get; private set; }
        protected IAnimator RotateAnimator { get; private set; }
        protected IAnimator ScaleAnimator { get; private set; }
        protected Vector3 TargetPosition
        {
            get
            {
                Vector3 pos = Menu.IsActive ? _initialPosition : Vector3.zero;
                if (Menu.IsActive && _movesAsRadiusChanging)
                {
                    //pos = _posDelta + _dirToCenter * (Menu.Radius );
                    pos = _initialPosition + _dirToCenter * _radDelta;
                }
                return pos;
            }
        }
        protected Quaternion TargetRotation
        {
            get
            {
                if (Menu.IsActive && _rotate)
                {
                    return Menu.Mover.TargetRotation;
                }
                return Menu.IsActive ? _initialRotation : Quaternion.identity;
            }
        }
        protected Vector3 TargetScale
        {
            get
            {
                Vector3 scale = Menu.IsActive ? _initialScale : Vector3.zero;
                if (Menu.IsActive && _scalesAsRadiusChanging)
                {
                    scale = Vector3.one * (Menu.Scaler.UniformScale + _scaleDelta);
                }
                return scale;
            }
        }

        private float _radDelta => Menu.Radius - _initRad;
        private RadialMenu _menu;
        private Vector3 _initialPosition, _initialScale;
        private Quaternion _initialRotation;
        private Vector3 _dirToCenter;
        private float _initRad;
        private bool _init = false;
        private float _scaleDelta;

        internal abstract void Animate();
        internal abstract void SetPosition(Vector3 position);
        internal abstract void SetScale(Vector3 scale);
        internal abstract void SetLocalRotation(Quaternion targetRotation);

        public void Awake()
        {
            if (AttachedObj == null) AttachedObj = transform;
            bool springMovement = MoveSettings.AnimateUsing == Easing.NumericSpring;
            bool springRotation = RotateSettings.AnimateUsing == Easing.NumericSpring;
            bool springScale = ScaleSettings.AnimateUsing == Easing.NumericSpring;
            MoveAnimator = springMovement ? (IAnimator)new NumericSpring(MoveSettings) : (IAnimator)new AnimCurveLerper(MoveSettings);
            RotateAnimator = springRotation ? (IAnimator)new NumericSpring(RotateSettings) : (IAnimator)new AnimCurveLerper(RotateSettings);
            ScaleAnimator = springScale ? (IAnimator)new NumericSpring(ScaleSettings) : (IAnimator)new AnimCurveLerper(ScaleSettings);
            SetInitialPositionAndScale(AttachedObj.localPosition);
        }

        [Button]
        public void SnapToSelectedAndSave()
        {
            if (!Menu.Initialized) Menu.Init();
            Vector3 posLocal = Menu.HoveredItem.GetMenuRelativePos();
            Vector3 pos = Quaternion.Inverse(transform.localRotation) * posLocal;
            SetInitialPositionAndScale(pos);
            SetPosition(TargetPosition);
            SetLocalRotation(TargetRotation);
            SetScale(TargetScale);
        }

        [Button]
        public void SaveCurrentPositionAndScale()
        {
            SetInitialPositionAndScale(AttachedObj.localPosition);
            _init = true;
        }

        internal void InitPosAndScale()
        {
            if (Menu.InEditMode)
            {
                if (!_init) SaveCurrentPositionAndScale();
                SetPosition(TargetPosition);
                SetScale(TargetScale);
            }
        }
        protected void SetInitialPositionAndScale(Vector3 position)
        {
            _initRad = Menu.Radius;
            _initialScale = AttachedObj.localScale;
            _scaleDelta = AttachedObj.localScale.x - Menu.Scaler.UniformScale;
            _initialPosition = position;
            _dirToCenter = _initialPosition.normalized;
            _initialRotation = AttachedObj.localRotation;
        }
    }
}
