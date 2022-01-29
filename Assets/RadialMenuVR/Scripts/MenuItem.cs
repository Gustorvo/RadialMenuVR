using System;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class MenuItem : MenuItemBase, IPlaceble
    {
        [SerializeField] GameObject _icon;
        public GameObject Icon => _icon;
        public Vector3 PositionLocal => transform.localPosition;
        public Quaternion Rotation => transform.rotation;
        public Vector3 ScaleLocal => transform.localScale;
        public SpriteRenderer SpriteRenderer { get; private set; }
        public Vector3 InitPos { get; private set; } = Vector3.zero;
        public Vector3 InitScale { get; private set; } = Vector3.zero;
        public string ItemText { get; private set; } = string.Empty;
        public Sprite SpriteIcon { get; private set; }
        public int Index { get; private set; }


        //  private float _initRad = 0;

        private void Awake()
        {
            SpriteRenderer = Icon.GetComponent<SpriteRenderer>();
        }
        private void Start()
        {
            InitPos = Menu.Items.GetInitialPositions()[Index];
            InitScale = Menu.Items.GetInitialScales()[Index];
            if (SpriteIcon != null)
                SpriteRenderer.sprite = SpriteIcon;
            // _initRad = Menu.Radius;
        }

        public void SetColor(Color newColor)
        {
            if (SpriteRenderer == null) // for editor initialisation
                SpriteRenderer = Icon.GetComponent<SpriteRenderer>();
            SpriteRenderer.color = newColor;
        }

        public void Init(int index, string text, Sprite sprite)
        {
            Index = index;
            ItemText = text;
            SpriteIcon = sprite;
        }
        internal void InitFromPlaceable(IPlaceble placeble)
        {
            Init(placeble.Index, placeble.ItemText, placeble.SpriteIcon);
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
