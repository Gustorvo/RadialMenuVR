using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{    public class MenuItem : MonoBehaviour
    {
        [SerializeField] GameObject _icon;
        public GameObject Icon => _icon;
        public SpriteRenderer SpriteRenderer { get; private set; }

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
