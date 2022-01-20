using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class MenuAttachments : MonoBehaviour
    {
        [SerializeField] Attachment[] _attachments; // popuated in the inspector                

        void Update()
        {
            for (int i = 0; i < _attachments.Length; i++)
            {
                _attachments[i]?.Animate();
            }
        }
    }
}
