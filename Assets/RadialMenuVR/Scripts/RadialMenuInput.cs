using UnityEngine;

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

        public void BeginSelectItem() => _menu.SetSelected(false);
        public void EndSelectItem() => _menu.SetSelected(true);

        private void Awake()
        {
            _menu = GetComponent<RadialMenu>();
            _input?.OnTriggerPress.AddListener(BeginSelectItem);
            _input?.OnTriggerRelease.AddListener(EndSelectItem);
            _input?.OnPrimaryButtonPress.AddListener(ToggleMenu);
            _input?.OnPrimary2DAxisLeft.AddListener(() => ChangeMenuItem(-1));
            _input?.OnPrimary2DAxisRight.AddListener(() => ChangeMenuItem(1));
        }
        private void OnDestroy()
        {
            _input?.OnTriggerPress.RemoveAllListeners();
            _input?.OnTriggerRelease.RemoveAllListeners();
            _input?.OnPrimaryButtonPress.RemoveAllListeners();
            _input?.OnPrimary2DAxisLeft.RemoveAllListeners();
            _input?.OnPrimary2DAxisRight.RemoveAllListeners();
        }
    }
}
