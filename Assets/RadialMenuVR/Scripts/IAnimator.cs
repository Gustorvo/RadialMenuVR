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
        public void Animate(ref Vector3 curValue, Vector3 targetValue);
        public void Animate(ref float curValue, float targetValue);
    }

    [System.Serializable]
    public class AnimatorSettings
    {
        [field: SerializeField] 
        public Easing AnimateUsing { get; private set; } = Easing.NumericSpring;  
        
        [field: SerializeField, ShowIf("AnimateUsing", Easing.AnimationCurve), AllowNesting]
        public AnimationCurve curve { get; private set; } = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1)); // default linear curve
      
        [field: SerializeField, ShowIf("AnimateUsing", Easing.AnimationCurve), AllowNesting]
        public float duration { get; private set; } = 0.5f;
      
        [field: SerializeField, ShowIf("AnimateUsing", Easing.NumericSpring), AllowNesting] 
        public float damping { get; private set; } = 0.25f;
      
        [field: SerializeField, ShowIf("AnimateUsing", Easing.NumericSpring), AllowNesting] 
        public float frequency { get; private set; } = 4f;
      
        [field: SerializeField, ShowIf("AnimateUsing", Easing.NumericSpring), AllowNesting] 
        public bool allowSnapping { get; private set; } = true;       
    }
}
