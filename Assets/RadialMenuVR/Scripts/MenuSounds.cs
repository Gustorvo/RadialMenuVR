using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Implement velocity driven sound system
namespace Gustorvo.RadialMenu
{
[RequireComponent(typeof(RadialMenu))]
    public class MenuSounds : MonoBehaviour
    {
        [SerializeField] AudioClip _itemHoveredSound;
        [SerializeField] AudioClip _itemSelectedSound;
        [SerializeField] AudioClip _menuAppearSound;
        [SerializeField] AudioClip _menuDisappearSound;
        [SerializeField] bool _playOnHovered;
        [SerializeField] bool _playOnSelected;
        [SerializeField] bool _playOnAppear;
        [SerializeField] bool _playOnDisappear;

        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponent<RadialMenu>();
                return _menu;
            }
        }
        private RadialMenu _menu;
        private AudioSource _player;
     
        private void Awake()
        {
            _player = GetComponent<AudioSource>();
            Menu.OnItemHovered -= PlayHovered;
            Menu.OnItemSelected -= PlaySelected;
            Menu.OnToggleVisibility -= PlayVisibility;
            Menu.OnItemHovered += PlayHovered;
            Menu.OnItemSelected += PlaySelected;
            Menu.OnToggleVisibility += PlayVisibility;
        }

        private void Play(AudioClip clip)
        {
            if (!clip) return;
            _player.clip = clip;
            _player.Play();
        }

        private void PlayVisibility(bool active)
        {
            if (active && _playOnAppear) Play(_menuAppearSound);
            if (!active && _playOnDisappear) Play(_menuDisappearSound);
        }
        private void PlayHovered<T>(T obj)
        {
            if (_playOnHovered) Play(_itemHoveredSound);
        }
        private void PlaySelected(MenuItem item, bool confirmed)
        {
            if (confirmed && _playOnSelected) Play(_itemSelectedSound);
        }
    }
}
