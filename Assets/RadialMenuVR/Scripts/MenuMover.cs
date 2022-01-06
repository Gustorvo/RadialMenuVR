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
        [Header("Spring values")]
        [SerializeField] float _damping = 0.5f;
        [SerializeField] float _frequency = 3f;

        [Header("Popup values")]
        [SerializeField] float _lerpPopupTime;
        /// <summary>
        /// is menu currently moming/rotating?
        /// </summary>
        public bool IsMoving => _velocity != 0f;

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

        private void Awake()
        {
            _menu = GetComponent<RadialMenu>();
            _menu.OnRotated -= SetTargetAngle;
            _menu.OnRotated += SetTargetAngle;
            _menu.OnToggleVisibility -= SetTargetPosition;
            _menu.OnToggleVisibility += SetTargetPosition;
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            _targetPositions = _menu.ItemsPositions.ToArray();
            int count = _targetPositions.Length;
            _currentPositions = new Vector3[count];
            _curAngleZ = 0.0f;
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
            transform.position = _menu.Anchor.position;
            Rotate();
            PopUp();
        }

        private void Rotate()
        {
            // tween angle using numeric springing          
            _rotationSpring.SetGoing(ref _curAngleZ, ref _velocity, _targetAngleZ);
            Quaternion newRot = _menu.Anchor.rotation * Quaternion.AngleAxis(_curAngleZ, Vector3.forward);
            transform.rotation = newRot;
            // by setting forward vector, we essentially rotate each item to align with up vector
            _menu.ItemList.ForEach(i => i.Icon.transform.forward = _menu.Anchor.forward);
        }

        private void PopUp()
        {
            for (int i = 0; i < _menu.ItemList.Count; i++)
            {
                _popupSpring[i].SetGoing(ref _currentPositions[i], _targetPositions[i]);
                _menu.ItemList[i].Icon.transform.localPosition = _currentPositions[i];
            }
        }

        /// <summary>
        /// Returns the interpolator (t) between current and target rotation angles on Z axis
        /// </summary>
        /// <returns></returns>
        public float GetInterpolator() => Mathf.InverseLerp(_targetAngleZ - (_menu.DistToNextItemDeg * _step), _targetAngleZ, _curAngleZ);

        private void SetTargetAngle(int step)
        {
            _step = step;
            _targetAngleZ += _menu.DistToNextItemDeg * step;
        }

        private void SetTargetPosition()
        {
            if (!_init) Init();

            if (_popupCoroutine != null)
            {
                StopCoroutine(_popupCoroutine);
            }
            _popupCoroutine = StartCoroutine(SetTargetPositionsRoutine());
        }

        private IEnumerator SetTargetPositionsRoutine()
        {
            float delay = _lerpPopupTime / _menu.ItemList.Count;
            for (int i = 0; i < _menu.ItemList.Count; i++)
            {
                _targetPositions[i] = _menu.Active ? _menu.ItemsPositions[i] : Vector3.zero;
                yield return new WaitForSecondsRealtime(delay);
            }
        }

        #region Debug
        [Button("Next")]
        public void DebugNext() => _menu.ShiftItems(1);

        [Button("Previous")]
        public void DebugPrev() => _menu.ShiftItems(-1);
       
        [Button("ToggleVisibility")]
        public void DebugToggleVisibility() => _menu.ToogleVisibility();

        #endregion
    }
}
