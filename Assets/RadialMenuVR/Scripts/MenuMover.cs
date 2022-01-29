using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    public class MenuMover : MonoBehaviour
    {
        [SerializeField, OnValueChanged("UpdateLocalZAngleCallback"), /*EnableIf("InEditMode"),*/ Range(-180, 180)] public int _localZAngle = 0;
        [SerializeField] AnimatorSettings _moveSettings;
        [SerializeField] AnimatorSettings _rotateSettings;
        public Quaternion TargetRotation => _targetRotation;
        public bool IsRotating => _rotateAnimator.Active; //_angVelocity != 0f;
        public bool IsSlowing { get; private set; }
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponent<RadialMenu>();
                return _menu;
            }
        }

        private RadialMenu _menu;
        private Quaternion _targetRotation;
        private Quaternion _currentRotation;
        private Coroutine _toggleCoroutine, _rotateCoroutine;
        private Vector3[] _targetPositions;
        private Vector3[] _currentPositions;
        private int _step;
        private IAnimator[] _moveAnimator;
        private IAnimator _rotateAnimator;

        private void Awake()
        {
            Menu.OnStep -= SetTargetAngle;
            Menu.OnStep += SetTargetAngle;
            Menu.OnToggleVisibility -= ToggleMenuItems;
            Menu.OnToggleVisibility += ToggleMenuItems;
            Menu.OnMenuRebuild -= ToggleMenuItems;
            Menu.OnMenuRebuild += ToggleMenuItems;
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            _currentRotation = Quaternion.identity;
            _targetRotation = Quaternion.identity;
            int count = Menu.Items.Count;
            _moveAnimator = new IAnimator[count];
            _targetPositions = Menu.Items.GetTargetPositions();
            _currentPositions = new Vector3[count];
            // _curAngleZ = Menu.RotationOffset.eulerAngles.z;        
            _targetRotation = Quaternion.identity;
            //  _angVelocity = 0.0f;          
            bool springMovement = _moveSettings.AnimateUsing == Easing.NumericSpring;
            bool springRotation = _rotateSettings.AnimateUsing == Easing.NumericSpring;
            _rotateAnimator = springRotation ? (IAnimator)new NumericSpring(_rotateSettings) : (IAnimator)new AnimCurveLerper(_rotateSettings);

            for (int i = 0; i < Menu.Items.Count; i++)
            {
                _moveAnimator[i] = springMovement ? (IAnimator)new NumericSpring(_moveSettings) : (IAnimator)new AnimCurveLerper(_moveSettings);
            }
        }

        private void Update()
        {
            SetPositionAndRotation();
        }

        private void UpdateLocalZAngleCallback()
        {
            if (Menu.InEditMode)
            {
                Menu.transform.rotation = Quaternion.AngleAxis(_localZAngle, Vector3.forward);
            }
        }

        private void SetPositionAndRotation()
        {
            // Quaternion newAnimatedRot = Quaternion.AngleAxis(_curAngleZ, Vector3.forward);
            Quaternion newAnimatedRot = _currentRotation;
            Quaternion lookForwardRot = Quaternion.LookRotation(Menu.AnchorToFollow.forward);
            Quaternion newTargetRot = lookForwardRot * Quaternion.AngleAxis(_localZAngle, Vector3.forward); // fix this

            Menu.transform.rotation = newTargetRot;
            Menu.transform.position = Menu.AnchorToFollow.position;
            Menu.Items.SetLocalRotation(newAnimatedRot);
            Menu.Items.SetForwardForItem(Menu.HoveredItem);
        }

        private void SetTargetAngle(int step)
        {
            _step = step;
            //  _targetAngleZ += Menu.ItemsDeltaDistDeg * step;
            _targetRotation *= Quaternion.Euler(Vector3.forward * Menu.ItemsDeltaDistDeg * step);
            if (_rotateCoroutine == null)
                _rotateCoroutine = StartCoroutine(AnimateRotationAngleRoutine());
            // No need to stop the Coroutine if it's running.
            // animator will pick up the new target value automatically
        }

        private IEnumerator AnimateRotationAngleRoutine()
        {
            bool active = true;
            while (active)
            {
                _rotateAnimator.Animate(ref _currentRotation, _targetRotation);                
                // _angVelocity = _rotateAnimator.Velocity;              
                yield return null;
                active = _rotateAnimator.Active;
            }
            _rotateCoroutine = null;
        }
        private void ToggleMenuItems(bool active) => ToggleMenuItems();
        private void ToggleMenuItems()
        {
            _targetPositions = Menu.Items.GetTargetPositions();
            if (_toggleCoroutine != null) StopCoroutine(_toggleCoroutine);
            _toggleCoroutine = StartCoroutine(ToggleAnimationRoutine(_targetPositions));
        }

        private IEnumerator ToggleAnimationRoutine(Vector3[] target)
        {
            IsSlowing = false;
            while (!IsSlowing)
            {
                for (int i = 0; i < Menu.Items.Count; i++)
                {
                    if (Menu.IsActive) _moveAnimator[i].Animate(ref _currentPositions[i], target[i]);
                    else _moveAnimator[i].Animate(ref _currentPositions[i], target[i], true, true); // make critically damped system when toggling off
                }
                Menu.Items.SetPositions(_currentPositions);
                yield return null;
                IsSlowing = Array.TrueForAll(_moveAnimator, i => !i.Active);
            }
            _toggleCoroutine = null;
        }

        #region Debug
        private bool editor => Menu.InEditMode;
        [Button("Next"), DisableIf("editor")]
        public void DebugNext() => Menu.ShiftItems(1);

        [Button("Previous"), DisableIf("editor")]
        public void DebugPrev() => Menu.ShiftItems(-1);

        [Button("Select"), DisableIf("editor")]
        public void Select() => Menu.SetSelected(true);

        [Button("ToggleVisibility"), DisableIf("editor")]
        public void DebugToggleVisibility() => Menu.ToogleVisibility();

        #endregion
    }
}
