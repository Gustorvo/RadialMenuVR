using UnityEngine;

namespace Gustorvo.RadialMenu
{
    public abstract class Attachment : MonoBehaviour
    {
        [field: SerializeField] public AnimatorSettings MoveSettings { get; private set; }
        [field: SerializeField] public AnimatorSettings ScaleSettings { get; private set; }

        [SerializeField] protected bool _move = false;
        [SerializeField] protected bool _scale = false;
        protected RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponentInParent<RadialMenu>();
                return _menu;
            }
        }

        public IAnimator MoveAnimator { get; private set; }
        public IAnimator ScaleAnimator { get; private set; }

        private RadialMenu _menu;
        internal abstract void Animate();      

        public void Awake()
        {
            // init animators
            bool springMovement = MoveSettings.AnimateUsing == Easing.NumericSpring;
            bool springScale = ScaleSettings.AnimateUsing == Easing.NumericSpring;
            MoveAnimator = springMovement ? (IAnimator)new NumericSpring(MoveSettings) : (IAnimator)new AnimCurveLerper(MoveSettings);
            ScaleAnimator = springScale ? (IAnimator)new NumericSpring(ScaleSettings) : (IAnimator)new AnimCurveLerper(ScaleSettings);
        }
    }
}
