using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class ChosenIndicator : MonoBehaviour
    {
        [SerializeField] Transform _icon;

        public GameObject Icon => _icon.gameObject;
    }
}
