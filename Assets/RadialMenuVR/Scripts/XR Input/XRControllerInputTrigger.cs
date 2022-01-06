using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// Simulates XR controller input
/// </summary>
namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(XRControllerInput))]
    public class XRControllerInputTrigger : MonoBehaviour
    {
        private XRControllerInput _input;
        private void Awake()
        {
            _input = GetComponent<XRControllerInput>();
        }

                
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void TriggerPress() => _input.OnTriggerPress?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void TriggerRelease() => _input.OnTriggerRelease?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void GripPress() => _input.OnGripPress?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void GripRelease() => _input.OnGripRelease?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void Prim2DAxisPress() => _input.OnPrimary2DAxisPress?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void Prim2DAxisRelease() => _input.OnPrimary2DAxisRelease?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void Prim2DAxisRight() => _input.OnPrimary2DAxisRight?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void Prim2DAxisLeft() => _input.OnPrimary2DAxisLeft?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void Prim2DAxisUp() => _input.OnPrimary2DAxisUp?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void Prim2DAxisDown() => _input.OnPrimary2DAxisDown?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void Secondary2DAxisPress() => _input.OnSecondary2DAxisPress?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void Secondary2DAxisRelease() => _input.OnSecondary2DAxisRelease?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void PrimaryButtonPress() => _input.OnPrimaryButtonPress?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void SecondaryButtonPress() => _input.OnSecondaryButtonPress?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void MenuButtonPress() => _input.OnMenuButtonPress?.Invoke();
        [Button(enabledMode: EButtonEnableMode.Playmode)]
        void MenuButtonRelease() => _input.OnMenuButtonRelease?.Invoke();

    }
}
