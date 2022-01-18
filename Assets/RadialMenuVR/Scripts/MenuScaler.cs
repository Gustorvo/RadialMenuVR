using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    public class MenuScaler : MonoBehaviour
    {
        [SerializeField, OnValueChanged("OnScaleFactorChanged"), Range(1.1f, 2f)] float _upscaleSelectedFactor = 1.25f;
        [SerializeField, OnValueChanged("OnScaleFactorChanged"), Range(0f, 1f)] float _itemScaleFactor = 1f;
        [SerializeField, ReadOnly] float _itemUniformScale = 0f;
        [SerializeField] AnimatorSettings _scaleSettings;


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
        private IAnimator[] _scaleAnimator;
        private RadialMenu _menu;
        private Vector3[] _currentScales;

        private void Awake()
        {
            Menu.OnToggleVisibility -= Scale;
            Menu.OnToggleVisibility += Scale;
            Menu.OnStep -= Scale;
            Menu.OnStep += Scale;
            Menu.OnMenuRebuild -= Scale;
            Menu.OnMenuRebuild += Scale;
        }
        private void Start()
        {
            Init();
        }

        public void Init()
        {
            int numOfItems = Menu.Items.Count;
            _currentScales = new Vector3[numOfItems];
            _scaleAnimator = new IAnimator[numOfItems];
            bool spring = _scaleSettings.AnimateUsing == Easing.NumericSpring;
            for (int i = 0; i < numOfItems; i++)
            {
                _scaleAnimator[i] = spring ? (IAnimator)new NumericSpring(_scaleSettings) : (IAnimator)new AnimCurveLerper(_scaleSettings);
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

        public void Scale() => Scale(0);

        private void Scale(int step)
        {
            bool inEditorNotPlaying = Application.isEditor && !Application.isPlaying;

            if (inEditorNotPlaying)
            {
                _currentScales = Enumerable.Repeat(ItemsInitialScale, Menu.Items.Count).ToArray();
                _currentScales[Menu.ChosenIndex] *= _upscaleSelectedFactor;
                Menu.Items.SetScales(_currentScales);
                Menu.Indicator.InitPositionAndScale();
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(ScaleRoutine());
            }
        }

        private IEnumerator ScaleRoutine()
        {
            Vector3[] targetScales = Menu.Items.GetTargetScales();
            targetScales[Menu.ChosenIndex] *= ChosenUpscaleFactor;
            bool areSlowing = false;
            while (!areSlowing)
            {
                for (int i = 0; i < Menu.Items.Count; i++)
                {
                    _scaleAnimator[i].Animate(ref _currentScales[i], targetScales[i]);
                }
                Menu.Items.SetScales(_currentScales);
                areSlowing = Array.TrueForAll(_scaleAnimator, i => !i.Active);
                yield return null;
            }
        }

        public void OnScaleFactorChanged()
        {           
            Menu.Rebuild();
            Scale();            
        }
    }
}
