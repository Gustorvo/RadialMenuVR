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
        private bool _resting = false;
        public bool Active => !_resting;
        private float _sumVelocities => Mathf.Abs(_vx) + Mathf.Abs(_vy) + Mathf.Abs(_vz); // velocity vector values

        public NumericSpring(float damping, float frequency)
        {
            _damping = damping;
            _frequency = frequency * Mathf.PI;            
        }

        public void Activate(ref float curValue, float targetValue)
        {
            Activate(ref curValue, ref _velocity, targetValue, false);
        }
        public void Activate(ref Vector3 curValue, Vector3 targetValue, bool allowSnapToTarget = true)
        {
            _x = curValue.x; _y = curValue.y; _z = curValue.z;
            Activate(ref _x, ref _vx, targetValue.x, false);
            Activate(ref _y, ref _vy, targetValue.y, false);
            Activate(ref _z, ref _vz, targetValue.z, false);
            curValue.Set(_x, _y, _z);
            _resting = _sumVelocities < 0.00001f && Vector3.SqrMagnitude(curValue - targetValue) < 0.001f;
            if (allowSnapToTarget && _resting)
            {
                curValue = targetValue; //snap to target          
            }
        }

        // it may take very long time untill spring comes to rest...
        // to prevent this, set 'allowSnapToTarget' to 'true'              
        public void Activate(ref float curValue, ref float velocity, float targetValue, bool allowSnapToTarget = true)
        {
            _resting = Mathf.Abs(velocity) < 0.01f && Mathf.Abs( Mathf.Abs(targetValue) - Mathf.Abs(curValue)) < 0.001f;
            if (allowSnapToTarget && _resting)
            {
                curValue = targetValue; //snap to target
                velocity = 0f;
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
