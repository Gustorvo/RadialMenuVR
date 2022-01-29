using UnityEngine;

namespace Gustorvo.RadialMenu
{
    /// <summary>
    /// Menu item (to be placed in the menu)
    /// </summary>
    public interface IPlaceble
    {
        /// <summary>
        /// Index controlls the ordinal number of this item in the list
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Icon to be usen in the menu
        /// </summary>
        /// 
        public Sprite SpriteIcon { get; }

        /// <summary>
        /// Text to be displayed
        /// </summary>
        public string ItemText { get; }
    }
}
