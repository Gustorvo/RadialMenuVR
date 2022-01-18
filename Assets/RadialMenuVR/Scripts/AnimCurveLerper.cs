using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    /// <summary>
    /// Wrapper for some of the Lerp functions (Vector3 and float)
    /// using animation curves
    /// </summary>
    public class AnimCurveLerper : IAnimator
    {      
        private AnimatorSettings _settings;
        private float _journey;
        private float _percent;
        private float _curvePercent;
        private float _origin;
        private float _target;
        private float _prevValue;
        private float _velocity;
        private Vector3 _originVector3;
        private Vector3 _targetVector3;
        private bool _done => _curvePercent == 1f;
        public bool Active => !_done;

        public float Velocity { get => throw new NotImplementedException(); }

        public AnimCurveLerper(AnimatorSettings settings)
        {
            _settings = settings;         
        }
        
        public void Animate(ref Vector3 curValue, Vector3 targetValue)
        {
            if (targetValue != _targetVector3)
            {
                // reset
                _originVector3 = curValue;
                _targetVector3 = targetValue;
                _journey = 0f;
            }
            _journey = _journey + Time.deltaTime;
            _percent = Mathf.Clamp01(_journey / _settings.duration);
            _curvePercent = _settings.curve.Evaluate(_percent);

            curValue = Vector3.LerpUnclamped(_originVector3, _targetVector3, _curvePercent);
        }
        public void Animate(ref float curValue, /*ref float velocity,*/ float targetValue)
        {
            if (targetValue != _target)
            {
                // reset
                _origin = curValue;
                _target = targetValue;
                _journey = 0f;
            }
            _journey += Time.deltaTime;
            _percent = Mathf.Clamp01(_journey / _settings.duration);
            _curvePercent = _settings.curve.Evaluate(_percent);
            _prevValue = curValue;
            curValue = Mathf.LerpUnclamped(_origin, _target, _curvePercent);
            _velocity = _done ? 0f : (curValue - _prevValue) / Time.deltaTime;
        }
    }
}
