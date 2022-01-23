using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class MenuAttachments : MonoBehaviour
    {
        [SerializeField] AttachmentBase[] _attachments; // popuated in the inspector                

        void Update()
        {
            for (int i = 0; i < _attachments.Length; i++)
            {
                _attachments[i]?.Animate();
            }
        }

        internal void SetLocalRotation(Quaternion targetRotation)
        {
            for (int i = 0; i < _attachments.Length; i++)
            {
                _attachments[i]?.SetLocalRotation(targetRotation);
            }
        }

        [Button]
        private void SnapAllToChosen() => Array.ForEach(_attachments, a => a.SnapToChosenAndSave());

        internal void InitPositionAndScale() => Array.ForEach(_attachments, a => a.InitPosAndScale());

    }
}
