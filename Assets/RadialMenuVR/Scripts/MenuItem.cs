using System;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class MenuItem : MonoBehaviour
    {
        [SerializeField] GameObject _icon;
        public GameObject Icon => _icon;
        public SpriteRenderer SpriteRenderer { get; private set; }
        public int Id { get; internal set; }

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
