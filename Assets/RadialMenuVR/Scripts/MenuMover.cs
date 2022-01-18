using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    public class MenuMover : MonoBehaviour
    {
        [SerializeField] AnimatorSettings _moveSettings;
        [SerializeField] AnimatorSettings _rotateSettings; 
       
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
        private float _curAngleZ;
        private float _targetAngleZ;
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
            int count = Menu.Items.Count;
            _moveAnimator = new IAnimator[count];
            _targetPositions = Menu.Items.GetTargetPositions();
            _currentPositions = new Vector3[count];
            _curAngleZ = Menu.RotaionOffset.eulerAngles.z;
            _targetAngleZ = _curAngleZ;
            //  _angVelocity = 0.0f;          
            bool springMovement = _moveSettings.AnimateUsing == Easing.NumericSpring;
            bool springRotation = _rotateSettings.AnimateUsing  == Easing.NumericSpring;
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

        private void SetPositionAndRotation()
        {
            // --- move and rotate menu ---
            Quaternion newRot = Quaternion.AngleAxis(_curAngleZ, Vector3.forward);
            Menu.SetRotation(newRot);
            Menu.SetPosition();

            // by setting forward vector, we essentially rotate each item to align with up vector
            Menu.Items.SetForwardVector(Menu.AnchorToFollow.forward);

            // --- indicator ---
            //ToggleIndicator();
        }

        private void ToggleIndicator()
        {
            if (IsSlowing) return;
            float a = Menu.Active ? 0f : Menu.Radius;
            float b = Menu.Active ? Menu.Radius : 0f;
            float distToCenter = _currentPositions[0].y - 0f;
            float interpolator = Mathf.InverseLerp(a, b, distToCenter);
            Vector3 indicatorPos = Vector3.Lerp(Menu.Indicator.Position, Menu.Indicator.TargetPosition, interpolator);
            Menu.Indicator.SetPositions(indicatorPos);
        }

        /// <summary>
        /// Returns the interpolator (t) between current and target rotation angles on Z axis
        /// </summary>
        /// <returns></returns>
        public float GetInterpolator() => Mathf.InverseLerp(_targetAngleZ - (Menu.ItemsDelataDistDeg * _step), _targetAngleZ, _curAngleZ);

        private void SetTargetAngle(int step)
        {
            _step = step;
            _targetAngleZ += Menu.ItemsDelataDistDeg * step;
            if (_rotateCoroutine != null) StopCoroutine(_rotateCoroutine);
            _rotateCoroutine = StartCoroutine(AnimateRotatingRoutine());
        }

        private IEnumerator AnimateRotatingRoutine()
        {
            bool active = true;
            float time = 0f;
            while (active)
            {
                _rotateAnimator.Animate(ref _curAngleZ, _targetAngleZ);
                // _angVelocity = _rotateAnimator.Velocity;
                time += Time.deltaTime;
                yield return null;
                active = _rotateAnimator.Active;
            }
            Debug.Log("time: " + time);
        }

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
                    _moveAnimator[i].Animate(ref _currentPositions[i], target[i]);
                }
                Menu.Items.SetPositions(_currentPositions);
                yield return null;
                IsSlowing = Array.TrueForAll(_moveAnimator, i => !i.Active);
            }
        }       

        #region Debug
        [Button("Next")]
        public void DebugNext() => Menu.ShiftItems(1);

        [Button("Previous")]
        public void DebugPrev() => Menu.ShiftItems(-1);

        [Button("ToggleVisibility")]
        public void DebugToggleVisibility() => Menu.ToogleVisibility();

        #endregion
    }
}
