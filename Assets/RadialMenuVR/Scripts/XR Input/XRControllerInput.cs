// Mappings docs: https://docs.unity3d.com/2019.3/Documentation/Manual/xr_input.html 

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

namespace Gustorvo.RadialMenu
{
    public class XRControllerInput : MonoBehaviour
    {
        [SerializeField, Tooltip("Left or Right Hand.")]
        private XRNode XRController = XRNode.RightHand;

        [SerializeField, Tooltip("Input value below this threshold won't be registered")]
        private float minAxisThreshold = 0.15f;

        #region private varialbles
        private List<InputDevice> _controllers = new List<InputDevice>();
        private InputDevice _targetController;
        private bool _buttonValue = false;
        private Vector2 _axis2DValue = Vector2.zero;
        private float _axisValue = 0f;

        private bool _triggerButton = false;
        private float _triggerValue = 0.0f;
        private bool _gripButton = false;
        private float _gripValue = 0.0f;
        private bool _primary2DAxisButton = false;
        private Vector2 _primary2DAxisValue = Vector2.zero;
        private bool _secondary2DAxisButton = false;
        private Vector2 _secondary2DAxisValue = Vector2.zero;
        private bool _primaryButton = false;
        private bool _secondaryButton = false;
        private bool _menuButton = false;
        private bool _wasLeft;
        private bool _wasRight;
        private bool _wasUp;
        private bool _wasDown;
        #endregion

        #region Events
        public UnityEvent OnTriggerPress;
        public UnityEvent OnTriggerRelease;

        public UnityEvent OnGripPress;
        public UnityEvent OnGripRelease;

        public UnityEvent OnPrimary2DAxisPress;
        public UnityEvent OnPrimary2DAxisRelease;

        public UnityEvent OnPrimary2DAxisRight;
        public UnityEvent OnPrimary2DAxisLeft;
        public UnityEvent OnPrimary2DAxisUp;
        public UnityEvent OnPrimary2DAxisDown;

        public UnityEvent OnSecondary2DAxisPress;
        public UnityEvent OnSecondary2DAxisRelease;

        public UnityEvent OnPrimaryButtonPress;
        public UnityEvent OnPrimaryButtonRelease;

        public UnityEvent OnSecondaryButtonPress;
        public UnityEvent OnSecondaryButtonRelease;

        public UnityEvent OnMenuButtonPress;
        public UnityEvent OnMenuButtonRelease;
        #endregion

        #region Button
        private bool TriggerButtonAction
        {
            get => _triggerButton;
            set
            {
                if (value == _triggerButton) return;
                _triggerButton = value;
                if (value == true) OnTriggerPress?.Invoke();
                else OnTriggerRelease?.Invoke();
                // Debug.Log($"Trigger Press {_triggerButton} on {XRController}");
            }
        }

        private bool GripButtonAction
        {
            get => _gripButton;
            set
            {
                if (value == _gripButton) return;
                _gripButton = value;
                if (value == true) OnGripPress?.Invoke();
                else OnGripRelease?.Invoke();
                // Debug.Log($"Grip Press {_gripButton} on {XRController}");
            }
        }

        private bool Primary2DAxisButtonAction
        {
            get => _primary2DAxisButton;
            set
            {
                if (value == _primary2DAxisButton) return;
                _primary2DAxisButton = value;
                if (value == true) OnPrimary2DAxisPress?.Invoke();
                else OnPrimary2DAxisRelease?.Invoke();
                //  Debug.Log($"Primary 2D Axis Button Press {_primary2DAxisButton} on {XRController}");
            }
        }

        private bool Secondary2DAxisButtonAction
        {
            get => _secondary2DAxisButton;
            set
            {
                if (value == _secondary2DAxisButton) return;
                _secondary2DAxisButton = value;
                if (value == true) OnSecondary2DAxisPress?.Invoke();
                else OnSecondary2DAxisRelease?.Invoke();
                // Debug.Log($"Secondary 2D Axis Button Press {_secondary2DAxisButton} on {XRController}");
            }
        }

        private bool PrimaryButtonAction
        {
            get => _primaryButton;
            set
            {
                if (value == _primaryButton) return;
                _primaryButton = value;
                if (value == true) OnPrimaryButtonPress?.Invoke();
                else OnPrimaryButtonRelease?.Invoke();
                // Debug.Log($"Primary Button Press {_primaryButton} on {XRController}");
            }
        }

        private bool SecondaryButtonAction
        {
            get => _secondaryButton;
            set
            {
                if (value == _secondaryButton) return;
                _secondaryButton = value;
                if (value == true) OnSecondaryButtonPress?.Invoke();
                else OnSecondaryButtonRelease?.Invoke();
                // Debug.Log($"Secondary Button Press {_secondaryButton} on {XRController}");
            }
        }

        private bool MenuButtonAction
        {
            get => _menuButton;
            set
            {
                if (value == _menuButton) return;
                _menuButton = value;
                if (value == true) OnMenuButtonPress?.Invoke();
                else OnMenuButtonRelease?.Invoke();
                //Debug.Log($"Menu Button Press {_menuButton} on {XRController}");
            }
        }
        #endregion

        #region Axis
        private float TriggerValueAction
        {
            get => _triggerValue;
            set
            {
                if (value == _triggerValue) return;
                _triggerValue = value;
                //Debug.Log($"Trigger Value {Mathf.RoundToInt(_triggerValue * 10f) / 10f} on {XRController}");              
            }
        }

        private float GripValueAction
        {
            get => _gripValue;
            set
            {
                if (value == _gripValue) return;
                _gripValue = value;
                //  Debug.Log($"Grip Value {Mathf.RoundToInt(_gripValue * 10f) / 10f} on {XRController}");            
            }
        }
        #endregion

        #region 2D Axis
        private Vector2 Primary2DAxisValueAction
        {
            get => _primary2DAxisValue;
            set
            {
                if (value == _primary2DAxisValue) return;
                bool isLeft = Get2dAxisDirection(value, Vector2Direction.Left);
                bool isRight = Get2dAxisDirection(value, Vector2Direction.Right);
                bool isUp = Get2dAxisDirection(value, Vector2Direction.Up);
                bool isDown = Get2dAxisDirection(value, Vector2Direction.Down);

                _primary2DAxisValue = value;

                if (isLeft && isLeft != _wasLeft)
                {
                    OnPrimary2DAxisLeft?.Invoke();
                }
                else if (isRight && isRight != _wasRight)
                {
                    OnPrimary2DAxisRight?.Invoke();
                }
                else if (isUp && isUp != _wasUp)
                {
                    OnPrimary2DAxisUp?.Invoke();
                }
                else if (isDown && isDown != _wasDown)
                {
                    OnPrimary2DAxisDown?.Invoke();
                }
                _wasLeft = isLeft;
                _wasRight = isRight;
                _wasDown = isDown;
                _wasUp = isUp;

                // Debug.Log($"Primary2DAxis X value {Mathf.RoundToInt(_primary2DAxisValue.x * 10f) / 10f} on {XRController}");
                // Debug.Log($"Primary2DAxis Y value {Mathf.RoundToInt(_primary2DAxisValue.y * 10f) / 10f} on {XRController}");
            }
        }

        private Vector2 Secondary2DAxisValueAction
        {
            get => _secondary2DAxisValue;
            set
            {
                if (value == _secondary2DAxisValue) return;
                _secondary2DAxisValue = value;
                // Debug.Log($"Secondary2DAxis X value {Mathf.RoundToInt(_secondary2DAxisValue.x * 10f) / 10f} on {XRController}");       
            }
        }
        #endregion

        private void Start()
        {
            if (!_targetController.isValid)
            {
                GetTargetController();
            }
        }
        private void GetTargetController()
        {
            InputDevices.GetDevicesAtXRNode(XRController, _controllers);
            _targetController = _controllers.FirstOrDefault();
        }


        void Update()
        {
            if (!_targetController.isValid)
            {
                GetTargetController();
            }

            GetInput();
        }

        private void GetInput()
        {
            if (!_targetController.isValid) return;

            // trigger axis
            if (_targetController.TryGetFeatureValue(CommonUsages.trigger, out _axisValue))
            {
                if (_axisValue > minAxisThreshold) TriggerValueAction = _axisValue;
                else TriggerValueAction = 0f;
            }
            // grip axis
            if (_targetController.TryGetFeatureValue(CommonUsages.grip, out _axisValue))
            {
                if (_axisValue > minAxisThreshold) GripValueAction = _axisValue;
                else GripValueAction = 0f;
            }
            // trigger button      
            if (_targetController.TryGetFeatureValue(CommonUsages.triggerButton, out _buttonValue))
            {
                TriggerButtonAction = _buttonValue;
            }
            // grip button
            if (_targetController.TryGetFeatureValue(CommonUsages.gripButton, out _buttonValue))
            {
                GripButtonAction = _buttonValue;
            }
            // primary 2D Axis
            if (_targetController.TryGetFeatureValue(CommonUsages.primary2DAxis, out _axis2DValue))
            {
                if (Mathf.Abs(_axis2DValue.x) > minAxisThreshold || Mathf.Abs(_axis2DValue.y) > minAxisThreshold) Primary2DAxisValueAction = _axis2DValue;
                else Primary2DAxisValueAction = Vector2.zero;
            }
            // secondary 2D Axis
            if (_targetController.TryGetFeatureValue(CommonUsages.secondary2DAxis, out _axis2DValue))
            {
                if (Mathf.Abs(_axis2DValue.x) > minAxisThreshold || Mathf.Abs(_axis2DValue.y) > minAxisThreshold) Secondary2DAxisValueAction = _axis2DValue;
                else Secondary2DAxisValueAction = Vector2.zero;
            }
            // primary 2d axis button
            if (_targetController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out _buttonValue))
            {
                Primary2DAxisButtonAction = _buttonValue;
            }
            // secondary 2d axis button
            if (_targetController.TryGetFeatureValue(CommonUsages.secondary2DAxisClick, out _buttonValue))
            {
                Secondary2DAxisButtonAction = _buttonValue;
            }
            // primary button
            if (_targetController.TryGetFeatureValue(CommonUsages.primaryButton, out _buttonValue))
            {
                PrimaryButtonAction = _buttonValue;
            }
            // secondary button
            if (_targetController.TryGetFeatureValue(CommonUsages.secondaryButton, out _buttonValue))
            {
                SecondaryButtonAction = _buttonValue;
            }
            // menu button
            if (_targetController.TryGetFeatureValue(CommonUsages.menuButton, out _buttonValue))
            {
                MenuButtonAction = _buttonValue;
            }
        }

        private enum Vector2Direction
        {
            Left, Right, Up, Down,

            // not in use:
            LeftUp, RightUp, LeftDown, RightDown
        }
        private bool Get2dAxisDirection(Vector2 curAxis, Vector2Direction dir)
        {
            switch (dir)
            {
                case Vector2Direction.Left:
                    //if left and not a diagonal
                    if (curAxis.x < -0.65f &&
                        curAxis.y < 0.65f && curAxis.y > -0.65f)
                    {
                        //Debug.Log("Trackpad - Pressed Left");
                        return true;
                    }
                    break;
                case Vector2Direction.Right:
                    //if right and not a diagonal
                    if (curAxis.x > 0.65f &&
                        curAxis.y < 0.65f && curAxis.y > -0.65f)
                    {
                        //Debug.Log("Trackpad - Pressed Right");
                        return true;
                    }
                    break;
                case Vector2Direction.Up:
                    //if up and not a diagonal
                    if (curAxis.y > 0.65f &&
                        curAxis.x > -0.65f && curAxis.x < 0.65f)
                    {
                        //Debug.Log("Trackpad - Pressed up");
                        return true;
                    }
                    break;
                case Vector2Direction.Down:
                    //if down and not a diagonal
                    if (curAxis.y < -0.65f &&
                        curAxis.x > -0.65f && curAxis.x < 0.65f)
                    {
                        //Debug.Log("Trackpad - Pressed Down");
                        return true;
                    }
                    break;
                case Vector2Direction.LeftUp:
                    if (curAxis.x < -0.65f && curAxis.y > 0.65f)
                    {
                        //Debug.Log("Trackpad - Pressed LeftUp");
                        return true;
                    }
                    break;
                case Vector2Direction.RightUp:
                    if (curAxis.x > 0.65f && curAxis.y > 0.65f)
                    {
                        //Debug.Log("Trackpad - Pressed RightUp");
                        return true;
                    }
                    break;
                case Vector2Direction.LeftDown:
                    if (curAxis.x < -0.4f && curAxis.y < -0.65f)
                    {
                        //Debug.Log("Trackpad - Pressed LeftDown");
                        return true;
                    }
                    break;
                case Vector2Direction.RightDown:
                    if (curAxis.x > 0.4f && curAxis.y < -0.65f)
                    {
                        //Debug.Log("Trackpad - Pressed RightDown");
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}