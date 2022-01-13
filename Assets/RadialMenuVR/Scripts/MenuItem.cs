using System;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class MenuItem : MonoBehaviour
    {
        [SerializeField] GameObject _icon;
        public GameObject Icon => _icon;
        public Vector3 Position => transform.localPosition;
        public Quaternion Rotation => transform.rotation;
        public Vector3 Scale => transform.localScale;
        public SpriteRenderer SpriteRenderer { get; private set; }
        public int Index { get; internal set; }

        private void Awake()
        {
            SpriteRenderer = Icon.GetComponent<SpriteRenderer>();
        }       

        public void SetColor(Color newColor)
        {
            Icon.GetComponent<SpriteRenderer>().color = newColor;
        }        
    }
}
