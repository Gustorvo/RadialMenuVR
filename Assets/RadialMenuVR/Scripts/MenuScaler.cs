using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    [RequireComponent(typeof(MenuMover))]
    public class MenuScaler : MonoBehaviour
    {
        [SerializeField, Range(1.1f, 2f), OnValueChanged("OnScaleFactorChanged")] float _upscaleSelectedFactor = 1.25f;
        [SerializeField, OnValueChanged("OnScaleFactorChanged"), Range(0f, 1f)] float _itemScaleFactor = 1f;
        [SerializeField] float _animDuration;
        [SerializeField] AnimationCurve _animCurve;
        [SerializeField, ReadOnly] float _itemUniformScale = 0f;
        public Vector3 ItemsInitialScale { get; private set; } = Vector3.zero;
        public float ChosenUpscaleFactor => _upscaleSelectedFactor;
        public float UniformScale => _itemUniformScale;
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponent<RadialMenu>();
                return _menu;
            }
        }
        private AnimCurveLerper _animCurveScaler;
        private RadialMenu _menu;
        private MenuMover _mover;
        private Coroutine _scaleCoroutine;
        private Vector3[] _lerpedScales;

        private void Awake()
        {
            _mover = GetComponent<MenuMover>();
            Menu.OnToggleVisibility -= ToggleByScaling;
            Menu.OnToggleVisibility += ToggleByScaling;
            Menu.OnStep -= ScaleWhenRotating;
            Menu.OnStep += ScaleWhenRotating;
            Menu.OnMenuRebuild -= ScaleWhenRotating;
            Menu.OnMenuRebuild += ScaleWhenRotating;
        }
        private void Start()
        {
            Init();
        }

        //private void Update()
        //{
        //    Menu.Items.SetScales(_lerpedScales); // added

        //}

        public void Init()
        {
            _lerpedScales = new Vector3[Menu.Items.Count];
            _animCurveScaler = new AnimCurveLerper(_animCurve, _animDuration);
        }

        public void InitScale()
        {
            if (Menu.Items.Count < 2) return;
            if (Menu.RadiusAffectsScale)
            {
                _itemUniformScale = Menu.Items.DeltaDistance * _itemScaleFactor;
            }
            else _itemUniformScale = RadialMenu.minRadius * _itemScaleFactor;

            ItemsInitialScale = _itemUniformScale * Vector3.one;
        }

        private void ToggleByScaling()
        {
            StopAllCoroutines();
            _scaleCoroutine = StartCoroutine(ToggleVisibilityRoutine());
        }
        private void ScaleWhenRotating() => ScaleWhenRotating(0);

        private void ScaleWhenRotating(int step)
        {
            bool inEditorNotPlaying = Application.isEditor && !Application.isPlaying;

            if (inEditorNotPlaying)
            {
                _lerpedScales = Enumerable.Repeat(ItemsInitialScale, Menu.Items.Count).ToArray();
                _lerpedScales[Menu.ChosenIndex] *= _upscaleSelectedFactor;
                Menu.Items.SetScales(_lerpedScales);
                Menu.InitIndicatorPositionAndScale();
            }
            else
            {
                StopAllCoroutines();
                _scaleCoroutine = StartCoroutine(ScaleGraduallyRoutine());
            }
        }

        private IEnumerator ScaleGraduallyRoutine()
        {
            Vector3[] fromScale = Menu.Items.GetScales();
            Vector3[] toScale = Menu.Items.GetTargetScales();
            toScale[Menu.ChosenIndex] *= ChosenUpscaleFactor;
            float t = 0f;
            while (t != 1f)
            {
                t = _mover.GetInterpolator();
                for (int i = 0; i < Menu.Items.Count; i++)
                {
                    //_animCurveScaler.Activate()
                    _lerpedScales[i] = Vector3.Lerp(fromScale[i], toScale[i], t);
                }
                   Menu.Items.SetScales(_lerpedScales);
                yield return null;
            }
        }

        private IEnumerator ToggleVisibilityRoutine()
        {
            // toggle scale (between either "0" or "normal initial")
            float a = 0f;
            if (Menu.Items.TryGetItem(0, out MenuItem item))
                a = item.Position.y; // start
            float b = Menu.Active ? Menu.Radius : 0.0001f; //end
            float currentRadius = 0f;
            float t = 0f;
            Vector3[] fromScale = Menu.Items.GetScales();
            Vector3[] toScale = Menu.Items.GetTargetScales();
            toScale[Menu.ChosenIndex] *= ChosenUpscaleFactor;

            while (t != 1f)
            {
                currentRadius = item.Position.y;
                t = Mathf.InverseLerp(a, b, currentRadius);
                for (int i = 0; i < Menu.Items.Count; i++)
                {
                    _lerpedScales[i] = Vector3.Lerp(fromScale[i], toScale[i], t);
                }
                Menu.Items.SetScales(_lerpedScales);
                // scele indicator
                Menu.Indicator.SetScales(ItemsInitialScale);
                yield return null;
            }
        }

        public void OnScaleFactorChanged()
        {
            if (!Menu.Initialized) Menu.Init();
            InitScale();
            ScaleWhenRotating();
        }
    }
}
