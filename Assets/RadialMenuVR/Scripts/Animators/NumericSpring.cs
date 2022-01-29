using UnityEngine;

namespace Gustorvo.RadialMenu
{
    /// <summary>
    ///  implementation is based on: http://allenchou.net/2015/04/game-math-precise-control-over-numeric-springing/ 
    ///   ...and extended with Vector3 and Quaternion overloads! :)
    /// </summary>
    public class NumericSpring : IAnimator
    {
        private AnimatorSettings _settings;
        private float _velocity;
        private float _curValue;
        private float _tarValue;
        private float _fromValue;
        private float _x, _y, _z;
        private float _vx, _vy, _vz; // velocity vector values
        private bool _resting = false;
        private Quaternion _fromRotation = Quaternion.identity;
        private Quaternion _toRotation = Quaternion.identity;
        private Vector4 _velocityVec;

        public bool Active => !_resting;
        private float _sumVelocities => Mathf.Abs(_vx) + Mathf.Abs(_vy) + Mathf.Abs(_vz); // velocity vector values

        public float Velocity => throw new System.NotImplementedException();

        public NumericSpring(AnimatorSettings settings)
        {
            _settings = settings;
        }

        public void Spring(ref float curValue, float targetValue, bool removeOscillation = false, bool doubleFrequency = false)
        {
            Spring(ref curValue, ref _velocity, targetValue, removeOscillation, doubleFrequency);
        }
        public void Animate(ref Vector3 curValue, Vector3 targetValue, bool removeOscillation = false, bool doubleFrequency = false)
        {
            _x = curValue.x; _y = curValue.y; _z = curValue.z;
            Spring(ref _x, ref _vx, targetValue.x, removeOscillation, doubleFrequency);
            Spring(ref _y, ref _vy, targetValue.y, removeOscillation, doubleFrequency);
            Spring(ref _z, ref _vz, targetValue.z, removeOscillation, doubleFrequency);
            curValue.Set(_x, _y, _z);
            _resting = _sumVelocities < 0.00001f && Vector3.SqrMagnitude(curValue - targetValue) < 0.001f;           
        }
               
        public void Spring(ref float curValue, ref float velocity, float targetValue, bool removeOscillation, bool doubleFrequency)
        {
            float damping = removeOscillation ? 1f : _settings.Damping;
            float frequency = doubleFrequency ? _settings.Frequency * 2f : _settings.Frequency;
            float f = 1.0f + 2.0f * Time.deltaTime * damping * frequency;
            float oo = frequency * frequency;
            float hoo = Time.deltaTime * oo;
            float hhoo = Time.deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            float detX = f * curValue + Time.deltaTime * velocity + hhoo * targetValue;
            float detV = velocity + hoo * (targetValue - curValue);
            curValue = detX * detInv;
            velocity = detV * detInv;
        }

        public void Animate(ref Quaternion curRot, Quaternion targetRot)
        {
            if (curRot.EqualTo(targetRot)) return; // we're done!
            targetRot = Quaternion.Slerp(curRot, targetRot, 1f); // Slerp returns the shortest rotation!
                                                               
            Vector4 curRotValue = MyMath.ToVector4(curRot);
            Vector4 targetRotValue = MyMath.ToVector4(targetRot);
            Vector4 delta = targetRotValue - curRotValue;

            float f = 1.0f + 2.0f * Time.deltaTime * _settings.Damping * _settings.Frequency;
            float oo = _settings.Frequency * _settings.Frequency;
            float hoo = Time.deltaTime * oo;
            float hhoo = Time.deltaTime * hoo;
            float detInv = 1.0f / (f + hhoo);
            Vector4 detX = f * curRotValue + Time.deltaTime * _velocityVec + hhoo * targetRotValue;
            Vector4 detV = _velocityVec + hoo * delta;

            _velocityVec = detV * detInv;
            curRotValue = detX * detInv;

            if (_velocityVec.magnitude < Mathf.Epsilon && delta.magnitude < Mathf.Epsilon)
            {
                //snapp
                curRotValue = targetRotValue;
                _velocityVec = Vector4.zero;
            }

            Vector4.Normalize(curRotValue);
            curRot.Set(curRotValue.x, curRotValue.y, curRotValue.z, curRotValue.w);
        }

        // untested!
        public void Activate(ref Vector3 curValue, ref Vector3 velocity, Vector3 targetValue, bool allowSnapToTarget = true)
        {
           // _resting = velocity.sqrMagnitude < 0.01f && (targetValue - curValue).sqrMagnitude < 0.001f;
            //if (allowSnapToTarget && _resting)
            //{
            //    curValue = targetValue; //snap to target
            //    velocity = Vector3.zero;
            //}

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
