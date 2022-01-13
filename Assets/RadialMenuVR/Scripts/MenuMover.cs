using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    public class MenuMover : MonoBehaviour
    {
        [Header("Spring values")]
        [SerializeField] float _damping = 0.5f;
        [SerializeField] float _frequency = 3f;

        //[Header("Popup values")]
        //[SerializeField] float _lerpPopupTime;
        /// <summary>
        /// is menu currently moming/rotating?
        /// </summary>
        public bool IsMoving => _velocity != 0f;
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponent<RadialMenu>();
                return _menu;
            }
        }
        private RadialMenu _menu;
        private float _velocity;
        private float _curAngleZ;
        private float _targetAngleZ;
        private Coroutine _popupCoroutine;
        private Vector3[] _targetPositions;
        private Vector3[] _currentPositions;
        private bool _init = false;
        private int _step;
        private NumericSpring _rotationSpring;
        private NumericSpring[] _popupSpring;
        private bool _popUp;

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
            _velocity = 0.0f;
            _rotationSpring = new NumericSpring(_damping, _frequency);
            _popupSpring = new NumericSpring[count];
            for (int i = 0; i < _popupSpring.Length; i++)
            {
                _popupSpring[i] = new NumericSpring(_damping * 2f, _frequency * 2f);
            }
            _init = true;
        }


        private void Update()
        {
            SetPositionAndRotation();
            PopUp();
        }

        private void SetPositionAndRotation()
        {
            // tween angle using numeric springing          
            _rotationSpring.SetGoing(ref _curAngleZ, ref _velocity, _targetAngleZ);
            Quaternion newRot = Quaternion.AngleAxis(_curAngleZ, Vector3.forward);
            Menu.SetPosition();
            Menu.SetRotation(newRot);
            // by setting forward vector, we essentially rotate each item to align with up vector
            Menu.Items.SetForwardVector(Menu.AnchorToFollow.forward);
        }

        private void PopUp()
        {
            if (!_popUp) return;
            LerpIndicator();

            // calculate new position(s) every frame by modifying _currentPositions array
            for (int i = 0; i < Menu.Items.Count; i++)
            {
                _popupSpring[i].SetGoing(ref _currentPositions[i], _targetPositions[i]);                
            }
            // apply new position(s)
            Menu.Items.SetPositions(_currentPositions);
            
            // stop springing
            if (_currentPositions[0] == _targetPositions[0]) _popUp = false;
        }

        private void LerpIndicator()
        {
            float a = Menu.Active ? 0f : Menu.Radius;
            float b = Menu.Active ? Menu.Radius : 0f;
            float distToCenter = Vector3.Distance(_currentPositions[0], Vector3.zero);
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
        }

        private void SetTargetPosition()
        {
            if (!_init) Init();
            _targetPositions = Menu.Items.GetTargetPositions();           
            _popUp = true;
        }

        //private IEnumerator SetDelayedTargetPositionsRoutine()
        //{
        //    float delay = _lerpPopupTime / Menu.Items.Count;
        //    for (int i = 0; i < Menu.Items.Count; i++)
        //    {
        //       // _targetPositions[i] = Vector3.zero;
        //        yield return new WaitForSecondsRealtime(delay);
        //    }
        //}

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
