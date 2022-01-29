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
        [SerializeField] bool _scaleSelected = false;
        [SerializeField, OnValueChanged("OnScaleFactorChanged"), Range(1.1f, 2f)] float _upscaleHoveredFactor = 1.25f;
        [SerializeField, OnValueChanged("OnScaleFactorChanged"), Range(0f, 1f)] float _itemScaleFactor = 1f;
        [SerializeField, ReadOnly] float _itemUniformScale = 0f;
        [SerializeField] AnimatorSettings _scaleSettings;


        public Vector3 ItemsInitialScale { get; private set; } = Vector3.zero;
        public float HoveredUpscaleFactor => _upscaleHoveredFactor;
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
        private bool _init;

        private void Awake()
        {
            Menu.OnToggleVisibility -= ScaleAll;
            Menu.OnToggleVisibility += ScaleAll;
            Menu.OnStep -= ScaleAll;
            Menu.OnStep += ScaleAll;
            Menu.OnMenuRebuild -= ScaleAll;
            Menu.OnMenuRebuild += ScaleAll;
            Menu.OnItemSelected -= ScaleSelected;
            Menu.OnItemSelected += ScaleSelected;
        }

        private void ScaleSelected(MenuItem item, bool confirmed)
        {
            if (!_scaleSelected) return;
            if (!_init) Init();
            StopAllCoroutines();
            StartCoroutine(ScaleSelectedRoutine(confirmed, item));
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
            _init = true;
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

        public void ScaleAll() => ScaleAll(0);

        private void ScaleAll<T>(T obj)
        {
            bool inEditorNotPlaying = Application.isEditor && !Application.isPlaying;

            if (inEditorNotPlaying)
            {
                _currentScales = Enumerable.Repeat(ItemsInitialScale, Menu.Items.Count).ToArray();
                _currentScales[Menu.HoveredIndex] *= _upscaleHoveredFactor;
                Menu.Items.SetScales(_currentScales);
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(ScaleAllRoutine());
            }
        }

        private IEnumerator ScaleAllRoutine()
        {
            Vector3[] targetScales = Menu.Items.GetTargetScales();
            targetScales[Menu.HoveredIndex] *= HoveredUpscaleFactor;
            bool areSlowing = false;
            bool menuDisapear;
            while (!areSlowing)
            {
                menuDisapear = Menu.IsActive;
                for (int i = 0; i < Menu.Items.Count; i++)
                {                    
                    _scaleAnimator[i].Animate(ref _currentScales[i], targetScales[i], !menuDisapear);
                }
                Menu.Items.SetScales(_currentScales);
                areSlowing = Array.TrueForAll(_scaleAnimator, i => !i.Active);
                yield return null;
            }
        }

        private IEnumerator ScaleSelectedRoutine(bool confirmed, MenuItem item)
        {
            float scaleFactor = confirmed ? HoveredUpscaleFactor : 0.75f;
            Vector3 newScale = item.ScaleLocal;
            Vector3 targetSelectedScale = item.InitScale * scaleFactor;
            bool speedUp = !confirmed;
            bool active = true;
            while (active)
            {
                _scaleAnimator[item.Index].Animate(ref newScale, targetSelectedScale, speedUp, true);
                Menu.Items.SetScale(item, newScale);
                active = _scaleAnimator[item.Index].Active;
                yield return null;
            }
        }

        public void OnScaleFactorChanged()
        {
            Menu.Rebuild();
            ScaleAll();
        }
    }
}
