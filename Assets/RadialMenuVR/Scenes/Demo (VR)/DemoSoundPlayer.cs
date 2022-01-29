using NaughtyAttributes;
using UnityEngine;
using System.Linq;

namespace Gustorvo.RadialMenu
{
    public class DemoSoundPlayer : MonoBehaviour
    {
        [SerializeField] RadialMenu _menu;
        [SerializeField] AudioClip[] _sounds;

        private AudioSource _player;
        private IPlayable[] _playables;

        private void Awake()
        {
            _player = GetComponent<AudioSource>();
        }

        private void Subscribe()
        {
            _menu.OnItemSelected -= PlaySound;
            _menu.OnItemSelected += PlaySound;
            _menu.OnItemHovered -= PlaySound;
            _menu.OnItemHovered += PlaySound;
        }

        private void Start()
        {
            Init();
            Subscribe();
        }

        private void Init()
        {
            _playables = new IPlayable[_sounds.Length];
            for (int i = 0; i < _sounds.Length; i++)
            {
                IPlayable sound = new Playable(_sounds[i], i);
                _playables[i] = sound;
            }
        }

        private void PlaySound(MenuItem selectedItem) => PlaySound(selectedItem, true);

        private void PlaySound(MenuItem selectedItem, bool confirmed)
        {
            if (!confirmed) return;
            int i = Mathf.Clamp(selectedItem.Index, 0, _playables.Length);
            _player.clip = _playables[i].Clip;
            _player.Play();
            Debug.Log($"Playing {_player.clip.name}");
        }

        [Button]
        private void CreateMenuOfSounds()
        {
            Init();
            var placeableItems = _playables.Where(i => i is IPlaceble).Select(i => i as IPlaceble).ToArray();
            if (placeableItems.Length >= 2)
            {
                _menu.Items.CreateFromArray(placeableItems);
            }
        }

        private void OnDestroy()
        {
            _menu.OnItemSelected -= PlaySound;
        }
    }
}
