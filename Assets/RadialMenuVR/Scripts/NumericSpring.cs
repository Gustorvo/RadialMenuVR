using UnityEngine;

namespace Gustorvo.RadialMenu
{
    /// <summary>
    ///  based on: http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/
    /// </summary>
    public class NumericSpring
    {
        private float _damping;
        private float _frequency;
        private float _velocity;       
        private float _x, _y, _z;
        private float _vx, _vy, _vz; // velocity vector values     
    
        public NumericSpring(float damping, float frequency)
        {
            _damping = damping;
            _frequency = frequency * Mathf.PI;           
        }

        public void SetGoing(ref float curValue, float targetValue)
        {
            SetGoing(ref curValue, ref _velocity, targetValue);
        }
        public void SetGoing(ref Vector3 curValue, Vector3 targetValue)
        {
            _x = curValue.x; _y = curValue.y; _z = curValue.z;
            SetGoing(ref _x, ref _vx, targetValue.x);
            SetGoing(ref _y, ref _vy, targetValue.y);
            SetGoing(ref _z, ref _vz, targetValue.z);
            curValue.Set(_x, _y, _z);
        } 

        public void SetGoing(ref float curValue, ref float velocity, float targetValue)
        {
            // it may take very long time untill spring comes to rest...
            // to prevent this, we are checking if the current value is close enought to the target
            // as well as moving speed (velocity) is low enought
            // and snap to target if such the case
            if (Mathf.Abs(curValue - targetValue) < 0.0001f && velocity < 0.0001f)
            {
                // snap to target
                curValue = targetValue;
                velocity = 0f;
                return;
            }

            float f = 1.0f + 2.0f * Time.deltaTime * _damping * _frequency;
            float oo = _frequency * _frequency;
            float hoo = Time.deltaTime * oo;
            float hhoo = Time.deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            float detX = f * curValue + Time.deltaTime * velocity + hhoo * targetValue;
            float detV = velocity + hoo * (targetValue - curValue);
            curValue = detX * detInv;
            velocity = detV * detInv;
        }
    }
}
