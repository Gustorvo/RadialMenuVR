using NaughtyAttributes;
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

        [Header("Popup values")]
        [SerializeField] float _lerpPopupTime;
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

        private void Awake()
        {
            Menu.OnRotated -= SetTargetAngle;
            Menu.OnRotated += SetTargetAngle;
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
            _targetPositions = Menu.ItemsInitialPositions.ToArray();
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
            SetPositionAndRotation();
            PopUp();
        }

        private void SetPositionAndRotation()
        {
            // tween angle using numeric springing          
            _rotationSpring.SetGoing(ref _curAngleZ, ref _velocity, _targetAngleZ);
            Quaternion newRot = Menu.Rotation * Quaternion.AngleAxis(_curAngleZ, Vector3.forward);
            Menu.Rotation = newRot;
            // by setting forward vector, we essentially rotate each item to align with up vector
            Menu.ItemList.ForEach(i => i.Icon.transform.forward = Menu.Anchor.forward);
            Menu.SetPosition();
        }

        private void PopUp()
        {
            for (int i = 0; i < Menu.ItemList.Count; i++)
            {
                _popupSpring[i].SetGoing(ref _currentPositions[i], _targetPositions[i]);
                Menu.ItemList[i].Icon.transform.localPosition = _currentPositions[i];
            }
        }

        /// <summary>
        /// Returns the interpolator (t) between current and target rotation angles on Z axis
        /// </summary>
        /// <returns></returns>
        public float GetInterpolator() => Mathf.InverseLerp(_targetAngleZ - (Menu.DistToNextItemDeg * _step), _targetAngleZ, _curAngleZ);

        private void SetTargetAngle(int step)
        {
            _step = step;
            _targetAngleZ += Menu.DistToNextItemDeg * step;
        }

        private void SetTargetPosition()
        {
            if (!_init) Init();

            if (_popupCoroutine != null)
            {
                StopCoroutine(_popupCoroutine);
            }
            if (Menu.Active)
            {
                // set target positions immediately for activation
                _targetPositions = Menu.ItemsInitialPositions.ToArray();
            }
            else
            { // otherwise (for deactivation) set delayed target position for smooth sequential animation of each item
                _popupCoroutine = StartCoroutine(SetDelayedTargetPositionsRoutine());
            }
        }

        private IEnumerator SetDelayedTargetPositionsRoutine()
        {
            float delay = _lerpPopupTime / Menu.ItemList.Count;
            for (int i = 0; i < Menu.ItemList.Count; i++)
            {
                _targetPositions[i] = Vector3.zero;
                yield return new WaitForSecondsRealtime(delay);
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
