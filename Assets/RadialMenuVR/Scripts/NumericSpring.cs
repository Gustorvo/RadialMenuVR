using UnityEngine;

namespace Gustorvo.RadialMenu
{
    /// <summary>
    ///  based on: http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/
    /// </summary>
    public class NumericSpring : IAnimator
    {        
        private AnimatorSettings _settings;
        private float _velocity;
        private float _x, _y, _z;
        private float _vx, _vy, _vz; // velocity vector values
        private bool _resting = false;
       
        public bool Active => !_resting;
        private float _sumVelocities => Mathf.Abs(_vx) + Mathf.Abs(_vy) + Mathf.Abs(_vz); // velocity vector values

        public float Velocity => throw new System.NotImplementedException();

        public NumericSpring(AnimatorSettings settings)
        {
            _settings = settings;           
        }      

        public void Animate(ref float curValue, float targetValue)
        {
            Activate(ref curValue, ref _velocity, targetValue, false);
        }
        public void Animate(ref Vector3 curValue, Vector3 targetValue)
        {
            _x = curValue.x; _y = curValue.y; _z = curValue.z;
            Activate(ref _x, ref _vx, targetValue.x, false);
            Activate(ref _y, ref _vy, targetValue.y, false);
            Activate(ref _z, ref _vz, targetValue.z, false);
            curValue.Set(_x, _y, _z);
            _resting = _sumVelocities < 0.00001f && Vector3.SqrMagnitude(curValue - targetValue) < 0.001f;
            if (_settings.AllowSnapping && _resting)
            {
                curValue = targetValue; //snap to target          
            }
        }

        // it may take very long time untill spring comes to rest...
        // to prevent this, set 'allowSnapToTarget' to 'true'              
        public void Activate(ref float curValue, ref float velocity, float targetValue, bool allowSnapToTarget = true)
        {
            _resting = Mathf.Abs(velocity) < 0.01f && Mathf.Abs(Mathf.Abs(targetValue) - Mathf.Abs(curValue)) < 0.001f;
            if (allowSnapToTarget && _resting)
            {
                curValue = targetValue; //snap to target
                velocity = 0f;
            }

            float f = 1.0f + 2.0f * Time.deltaTime * _settings.Damping * _settings.Frequency;
            float oo = _settings.Frequency * _settings.Frequency;
            float hoo = Time.deltaTime * oo;
            float hhoo = Time.deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            float detX = f * curValue + Time.deltaTime * velocity + hhoo * targetValue;
            float detV = velocity + hoo * (targetValue - curValue);
            curValue = detX * detInv;
            velocity = detV * detInv;
        }

        public void Activate(ref Vector3 curValue, ref Vector3 velocity, Vector3 targetValue, bool allowSnapToTarget = true)
        {
            _resting = velocity.sqrMagnitude < 0.01f && (targetValue - curValue).sqrMagnitude < 0.001f;
            if (allowSnapToTarget && _resting)
            {
                curValue = targetValue; //snap to target
                velocity = Vector3.zero;
            }

            float f = 1.0f + 2.0f * Time.deltaTime * _settings.Damping * _settings.Frequency;
            float oo = _settings.Frequency * _settings.Frequency;
            float hoo = Time.deltaTime * oo;
            float hhoo = Time.deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector3 detX = f * curValue + Time.deltaTime * velocity + hhoo * targetValue;
            Vector3 detV = velocity + hoo * (targetValue - curValue);
            curValue = detX * detInv;
            velocity = detV * detInv;
        }
    }
}
