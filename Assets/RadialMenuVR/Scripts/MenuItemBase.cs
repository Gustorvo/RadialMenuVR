using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public abstract class MenuItemBase : MonoBehaviour
    {
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponentInParent<RadialMenu>();
                return _menu;
            }
        }
        private RadialMenu _menu;
    }
}
