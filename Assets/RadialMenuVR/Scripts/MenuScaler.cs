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
        [SerializeField] float _animDuration = 0.5f;
        [SerializeField, CurveRange(0, 0, 1, 1)] AnimationCurve _animCurve; // add defaults anim curve
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
        private AnimCurveLerper[] _animCurveScaler;
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
            int numOfItems = Menu.Items.Count;
            _lerpedScales = new Vector3[numOfItems];
            _animCurveScaler = new AnimCurveLerper[numOfItems];
            for (int i = 0; i < numOfItems; i++)
            {
                _animCurveScaler[i] = new AnimCurveLerper(_animCurve, _animDuration);
            }
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

        // increase selected/chosen item and decrease the rest of the menu items gradually as we rotate the menu
        private IEnumerator ScaleGraduallyRoutine()
        {
            Vector3[] toScale = Menu.Items.GetTargetScales();
            toScale[Menu.ChosenIndex] *= ChosenUpscaleFactor;
            bool areSlowing = false;
            while (!areSlowing)
            {
                for (int i = 0; i < Menu.Items.Count; i++)
                {
                    _animCurveScaler[i].Activate(ref _lerpedScales[i], toScale[i]);
                }
                Menu.Items.SetScales(_lerpedScales);
                areSlowing = Array.TrueForAll(_animCurveScaler, i => !i.Active);
                yield return null;
            }
        }

        // toggle scale (between either "0" or "normal initial")
        // the scale is lerped, whete the interpolator (t) is calculated by the current radius of the menu circle
        // the smaller the radius, the smaller the scale (and vice versa)
        private IEnumerator ToggleVisibilityRoutine()
        {
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
                Menu.Indicator.SetScales(_lerpedScales[0]);
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
