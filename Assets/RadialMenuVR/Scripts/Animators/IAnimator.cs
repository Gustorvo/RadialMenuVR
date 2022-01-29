using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public interface IAnimator
    {
        public float Velocity { get; }
        public bool Active { get; }
        public void Animate(ref Quaternion curRot, Quaternion targetRot);
        public void Animate(ref Vector3 curValue, Vector3 targetValue, bool removeOscillation = false, bool doubleFrequency = false);
        public void Spring(ref float curValue, float targetValue, bool removeOscillation = false, bool doubleFrequency = false);
    }

    [System.Serializable]
    public class AnimatorSettings
    {
        [field: SerializeField]
        public Easing AnimateUsing { get; private set; } = Easing.NumericSpring;

        [field: SerializeField, ShowIf("AnimateUsing", Easing.AnimationCurve), AllowNesting]
        public AnimationCurve Curve { get; private set; } = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1)); // default linear curve

        [field: SerializeField, ShowIf("AnimateUsing", Easing.AnimationCurve), AllowNesting]
        public float Duration { get; private set; } = 0.5f;

        [field: SerializeField, ShowIf("AnimateUsing", Easing.NumericSpring), AllowNesting]
        public float Damping { get; private set; } = 0.5f;

        [SerializeField, ShowIf("AnimateUsing", Easing.NumericSpring), AllowNesting] 
        private float _frequency = 1f;

        [field: SerializeField, ShowIf("AnimateUsing", Easing.NumericSpring), AllowNesting]
        public bool AllowSnapping { get; private set; } = false;

        public float Frequency
        {
            get => _frequency * Mathf.PI * 2f;
            set => _frequency = value;
        }
    }
}
