using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public class AnimCurveLerper
    {
        private AnimationCurve _curve;
        private float _duration;
        private float _journey;
        private float _percent;
        private float _curvePercent;
        private float _origin;
        private float _target;
        private float _prevValue;
        private Vector3 _originVector3;
        private Vector3 _targetVector3;
        private bool _done => _curvePercent == 1f;
        public bool Active => !_done;

        public AnimCurveLerper(AnimationCurve curve, float duration)
        {
            _curve = curve;
            _duration = duration;
        }

        public void Activate(ref Vector3 curValue, Vector3 targetValue)
        {
            if (targetValue != _targetVector3)
            {
                // reset
                _originVector3 = curValue;
                _targetVector3 = targetValue;
                _journey = 0f;
            }
            _journey = _journey + Time.deltaTime;
            _percent = Mathf.Clamp01(_journey / _duration);
            _curvePercent = _curve.Evaluate(_percent);

            curValue = Vector3.LerpUnclamped(_originVector3, _targetVector3, _curvePercent);
        }

        public void Activate(ref float curValue, ref float velocity, float targetValue)
        {           
            if (targetValue != _target)
            {
                // reset
                _origin = curValue;
                _target = targetValue;               
                _journey = 0f;
            }
            _journey += Time.deltaTime;
            _percent = Mathf.Clamp01(_journey / _duration);
            _curvePercent = _curve.Evaluate(_percent);
            _prevValue = curValue;
            curValue = Mathf.LerpUnclamped(_origin, _target, _curvePercent);
            velocity = _done ? 0f : (curValue - _prevValue) / Time.deltaTime;
        }
    }
}
