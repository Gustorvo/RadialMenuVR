using System;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class MenuItem : MenuItemBase
    {
        [SerializeField] GameObject _icon;     
        public GameObject Icon => _icon;
        public Vector3 PositionLocal => transform.localPosition;      
        public Quaternion Rotation => transform.rotation;
        public Vector3 ScaleLocal => transform.localScale;
        public SpriteRenderer SpriteRenderer { get; private set; }
        public int Index { get; private set; }
        public string Text { get; private set; } = string.Empty;
        private Vector3 _initPos = Vector3.zero;
        private float _initRad = 0;

        private void Awake()
        {
            SpriteRenderer = Icon.GetComponent<SpriteRenderer>();            
        }
        private void Start()
        {
            _initPos = Menu.Items.GetInitialPositions()[Index];
            _initRad = Menu.Radius;
        }

        public void SetColor(Color newColor)
        {
            Icon.GetComponent<SpriteRenderer>().color = newColor;
        }

        public void Init(int index, string text)
        {
            Index = index;
            Text = text;
        }

        /// <summary>
        /// Returns item's local position relative to the Menu
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMenuRelativePos()
        {
            //float deltaRadius = Menu.Radius - _initRad;
            //return _initPos * deltaRadius;

            Vector3 relativePosLocal = Menu.Items.transform.localRotation * PositionLocal; //Menu.transform.InverseTransformPoint(Menu.Chosen.transform.position);
            Vector3 dirToCenter = Vector3.Normalize(relativePosLocal - Vector3.zero);
            return dirToCenter * Menu.Radius;
        }
    }
}
