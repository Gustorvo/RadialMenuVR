using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    public class MenuMover : MonoBehaviour
    {
        [Header("Rotation values")]
        [SerializeField] Easing RotateUsing = Easing.NumericSpring;
        [SerializeField, ShowIf("RotateUsing", Easing.NumericSpring)] float _angDamping = 0.5f;
        [SerializeField, ShowIf("RotateUsing", Easing.NumericSpring)] float _angFrequency = 3f;
        [SerializeField, ShowIf("RotateUsing", Easing.AnimationCurve), CurveRange(0, 0, 1, 1)] AnimationCurve _angAnimationCurve;
        [SerializeField, ShowIf("RotateUsing", Easing.AnimationCurve)] float _rotDuration = 0.5f;
        [SerializeField, ReadOnly] private float _angVelocity;

        [Header("Appear/Disapear values")]
        [SerializeField] Easing MoveUsing = Easing.AnimationCurve;
        [SerializeField, ShowIf("MoveUsing", Easing.NumericSpring)] float _linDamping = 0.5f;
        [SerializeField, ShowIf("MoveUsing", Easing.NumericSpring)] float _linFrequency = 3f;
        [SerializeField, ShowIf("MoveUsing", Easing.NumericSpring)] bool _randomizeSpringValues = true;
        [SerializeField, ShowIf("MoveUsing", Easing.NumericSpring), EnableIf("_randomizeSpringValues"), Range(0.1f, 0.5f)] float _randomDelta = 0.15f;
        [SerializeField, ShowIf("MoveUsing", Easing.AnimationCurve), CurveRange(0, 0, 1, 1)] AnimationCurve _linAnimationCurve;
        [SerializeField, ShowIf("MoveUsing", Easing.AnimationCurve)] float _popupDuration = 0.5f;

        public bool IsRotating => _angVelocity != 0f;
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
        private Coroutine _popupCoroutine, _rotateCoroutine;
        private Vector3[] _targetPositions;
        private Vector3[] _currentPositions;       
        private int _step;
        private NumericSpring _springRot;
        private NumericSpring[] _springToggling;
        private AnimCurveLerper _animCurveRot;
        private AnimCurveLerper[] _animCurveToggling;     

        private void Awake()
        {
            Menu.OnStep -= SetTargetAngle;
            Menu.OnStep += SetTargetAngle;
            Menu.OnToggleVisibility -= SetTargetPosition;
            Menu.OnToggleVisibility += SetTargetPosition;
            Menu.OnMenuRebuild -= SetTargetPosition;
            Menu.OnMenuRebuild += SetTargetPosition;
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            int count = Menu.Items.Count;
            _targetPositions = Menu.Items.GetTargetPositions();
            _currentPositions = new Vector3[count];
            _curAngleZ = Menu.RotaionOffset.eulerAngles.z;
            _targetAngleZ = _curAngleZ;
            _angVelocity = 0.0f;
            _springRot = new NumericSpring(_angDamping, _angFrequency);
            _animCurveRot = new AnimCurveLerper(_angAnimationCurve, _rotDuration);
            _springToggling = new NumericSpring[count];
            _animCurveToggling = new AnimCurveLerper[count];
            for (int i = 0; i < _springToggling.Length; i++)
            {
                // init animation curves
                _animCurveToggling[i] = new AnimCurveLerper(_linAnimationCurve, _popupDuration);

                // init numeric springs
                float damping = _randomizeSpringValues ? Randomize(_linDamping, _randomDelta) : _linDamping;
                float frequency = _randomizeSpringValues ? Randomize(_linFrequency, _randomDelta) : _linFrequency;
                _springToggling[i] = new NumericSpring(damping, frequency);
            }          
        }

        private float Randomize(float value, float offset) => UnityEngine.Random.Range(value - offset, value + offset);

        private void Update()
        {
            SetPositionAndRotation();
        }

        private void SetPositionAndRotation()
        {
            // --- move and rotate menu ---
            Quaternion newRot = Quaternion.AngleAxis(_curAngleZ, Vector3.forward);
            Menu.SetPosition();
            Menu.SetRotation(newRot);

            // --- move and rotate menu items
            if (!IsSlowing)
            {
                Menu.Items.SetPositions(_currentPositions);// apply new position(s)
            }
            // by setting forward vector, we essentially rotate each item to align with up vector
            Menu.Items.SetForwardVector(Menu.AnchorToFollow.forward);

            // --- indicator ---
            LerpIndicator();
        }

        private void LerpIndicator()
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
            while (active)
            {
                if (RotateUsing == Easing.NumericSpring)
                {
                    _springRot.Activate(ref _curAngleZ, ref _angVelocity, _targetAngleZ);
                }
                else // animation curve
                {
                    _animCurveRot.Activate(ref _curAngleZ, ref _angVelocity, _targetAngleZ);
                }
                yield return null;
                active = RotateUsing == Easing.NumericSpring ? _springRot.Active : _animCurveRot.Active;
            }
        }

        private void SetTargetPosition()
        {
            _targetPositions = Menu.Items.GetTargetPositions();

            if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);
            _popupCoroutine = StartCoroutine(AnimatePopupRoutine(_targetPositions));
        }

        private IEnumerator AnimatePopupRoutine(Vector3[] target)
        {
            IsSlowing = false;
            while (!IsSlowing)
            {
                for (int i = 0; i < Menu.Items.Count; i++)
                {
                    if (MoveUsing == Easing.NumericSpring)
                    {
                        _springToggling[i].Activate(ref _currentPositions[i], target[i]);
                    }
                    else //Easing.AnimationCurve
                    {
                        _animCurveToggling[i].Activate(ref _currentPositions[i], target[i]);
                    }
                }
                yield return null;

                if (MoveUsing == Easing.NumericSpring)
                {
                    IsSlowing = Array.TrueForAll(_springToggling, i => !i.Active);
                }
                else
                {
                    IsSlowing = Array.TrueForAll(_animCurveToggling, i => !i.Active);
                }
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
