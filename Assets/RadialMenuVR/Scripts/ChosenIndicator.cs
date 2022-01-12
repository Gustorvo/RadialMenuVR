using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenIndicator : MonoBehaviour
    {
        [SerializeField] Transform _icon;
        [SerializeField] RadialMenu _menu;
        public Vector3 TargetPosition { get; private set; }
        public Vector3 StartPosition { get; private set; }
        public GameObject Icon => _icon.gameObject;
        public Vector3 Position => Icon.transform.localPosition;

        private void Awake()
        {
            _menu.OnToggleVisibility -= SetTargetPosition;
            _menu.OnToggleVisibility += SetTargetPosition;
            StartPosition = Icon.transform.localPosition;
        }

        public void SetPositon(Vector3 position)
        {
            Icon.transform.localPosition = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void SetScale(Vector3 scale)
        {
            Icon.transform.localScale = scale;
        }
        public void SetTargetPosition()
        {
            Vector3 target = _menu.Active ? StartPosition : Vector3.zero;
            TargetPosition = target;
        }
        private void OnDestroy()
        {
            _menu.OnToggleVisibility -= SetTargetPosition;
        }
    }
}
