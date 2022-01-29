using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class Playable: IPlayable, IPlaceble
    {  
        [field: SerializeField] public Sprite SpriteIcon { get; private set; }
        [field: SerializeField] public string ItemText { get; private set; }
        public int Index { get; private set; }
        public AudioClip Clip { get; private set; }

        public Playable(AudioClip audioClip, int index)
        {
            Clip = audioClip;
            Index = index;
            ItemText = audioClip.name;
            SpriteIcon = null;
        }
    }

    public interface IPlayable
    {
        public AudioClip Clip {get; }       
    }
}
