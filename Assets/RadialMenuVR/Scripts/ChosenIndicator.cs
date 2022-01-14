using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenIndicator : MonoBehaviour, IMovable
    {
        [SerializeField] Transform _icon;
        public Vector3 TargetPosition => Menu.Active ? InitialPosition : Vector3.zero;
        private Vector3 InitialPosition { get; set; }
        public GameObject Icon => _icon.gameObject;
        public Vector3 Position => Icon.transform.localPosition;
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponentInParent<RadialMenu>();
                return _menu;
            }
        }

        private RadialMenu _menu;
        private void Awake()
        {
            InitialPosition = Icon.transform.localPosition;
        }
        public void SetRotation(Quaternion rotation) => transform.rotation = rotation;
        public void SetPositions(Vector3 position) => Icon.transform.localPosition = position;
        public void SetScales(Vector3 scale) => Icon.transform.localScale = scale;
        public void SetForwardVector(Vector3 forward) => Icon.transform.forward = forward;
        public void SetPositions(Vector3[] positions) => throw new System.NotImplementedException();
        public void SetScales(Vector3[] scales) => throw new System.NotImplementedException();

    }
}
