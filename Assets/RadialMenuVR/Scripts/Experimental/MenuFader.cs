using UnityEngine;

namespace Gustorvo.RadialMenu
{
    /// <summary>
    ///  handles visibility for the carousel menu
    ///  not in used since scaler class replaces the functionality
    /// </summary>
    [RequireComponent(typeof(RadialMenu))]
    public class MenuFader : MonoBehaviour
    {
        [SerializeField] private float _lerpOutTime = 1.5f; // dissapear time
        [SerializeField] private float _lerpInTime = .5f; // appear time
        [SerializeField] private bool _isThumbstichTouching = true;
        [SerializeField] private float _maxTimeActive = 1.5f; // max time when active

        private float _lerpTime => _activate ? _lerpInTime : _lerpOutTime;
        private RadialMenu _menu;
        private MenuMover _mover;
        private bool _activated => _lerpedColor.a == 1f;
        private bool _activate = true;
        private float _currentLerpTime;
        private Color _aplhaColor, _lerpedColor, _nonAlphaColor;
        private float _timeActive;
        private bool _shouldReset; // should we reset the lerp ?   
        private bool _isMoving => _mover ? _mover.IsMoving : false;

        private void Awake()
        {
            _aplhaColor = Color.white;
            _nonAlphaColor = Color.white;

            _aplhaColor.a = 0f;
            _menu = GetComponent<RadialMenu>();
            _mover = GetComponent<MenuMover>();
            _menu.OnItemChosen -= MakeChosenVisibleImmediately;
            _menu.OnItemChosen += MakeChosenVisibleImmediately;
        }

        private void OnDestroy()
        {
            _menu.OnItemChosen -= MakeChosenVisibleImmediately;
        }
        void Update()
        {
            TakeInput();
            RegisterActiveness();
            ToggleVisibility();
        }

        private void RegisterActiveness()
        {
            // register time active
            if (_activated)
            {
                _timeActive += Time.deltaTime;
            }
            else
            {
                _timeActive = 0f;
            }

            // toggles active state

            if (_isThumbstichTouching || _isMoving)
            {
                _shouldReset = !_activate && _currentLerpTime == _lerpTime; // reset ?
                if (_shouldReset)
                {
                    _currentLerpTime = 0f;
                }
                _activate = true;
            }
            else
            {
                _shouldReset = _activate && _timeActive > _maxTimeActive; // reset ?
                if (_shouldReset)
                {
                    _currentLerpTime = 0f;
                    _activate = false;
                }
            }
        }

        private void TakeInput()
        {
           // if (OVRPlugin.hmdPresent)
          //  {
          //      _isThumbstichTouching = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, OVRInput.Controller.RTouch);
         //   }
        }

        private void MakeChosenVisibleImmediately(MenuItem item)
        {
            item.SetColor(_nonAlphaColor);
        }


        private void ToggleVisibility()
        {
            if (_activate)
            {
                _lerpedColor = Color.Lerp(_aplhaColor, _nonAlphaColor, GetT());
            }
            else
            {
                _lerpedColor = Color.Lerp(_nonAlphaColor, _aplhaColor, GetT());
            }

            for (int i = 0; i < _menu.ItemList.Count; i++)
            {
                if (i != _menu.ChosenIndex) // skip chosen since its aplha is already set
                {
                    _menu.ItemList[i].SetColor(_lerpedColor);
                }
            }
        }


        private float GetT()
        {
            float t = _currentLerpTime / _lerpTime;
            if (_activate)
            {
                t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f); // "ease in" with coslerp:
            }
            else
            {
                t = Mathf.Sin(t * Mathf.PI * 0.5f); //  "ease out" with sinlerp
            }
            _currentLerpTime += Time.deltaTime;
            if (_currentLerpTime > _lerpTime)
            {
                _currentLerpTime = _lerpTime;
            }
            return t;
        }
    }
}

