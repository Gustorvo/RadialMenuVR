using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public interface IMovable
    {
        public void SetPositions(Vector3 position);
        public void SetPositions(Vector3[] position);
        public void SetScales(Vector3[] scales);
        public void SetScales(Vector3 scale);
        public void SetRotation(Quaternion rotation);
        public void SetForwardVector(Vector3 forward);        
    }
}
