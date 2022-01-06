using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    public class RadialMenuInput : MonoBehaviour
    {
        [SerializeField] XRControllerInput _input;
        private RadialMenu _menu;

        public void ToggleMenu()
        {
            _menu.ToogleVisibility();
        }

        public void ChangeMenuItem(int step)
        {
            _menu.ShiftItems(step);
        }

        private void Awake()
        {
            _menu = GetComponent<RadialMenu>();
            _input?.OnTriggerPress.AddListener(ToggleMenu);
            _input?.OnPrimary2DAxisLeft.AddListener(() => ChangeMenuItem(-1));
            _input?.OnPrimary2DAxisRight.AddListener(() => ChangeMenuItem(1));
        }
        private void OnDestroy()
        {
            _input?.OnTriggerPress.RemoveListener(ToggleMenu);
            _input?.OnPrimary2DAxisLeft.RemoveListener(() => ChangeMenuItem(-1));
            _input?.OnPrimary2DAxisRight.RemoveListener(() => ChangeMenuItem(1));

        }
    }
}
