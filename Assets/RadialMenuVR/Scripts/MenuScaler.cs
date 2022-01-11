using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace Gustorvo.RadialMenu
{
    [RequireComponent(typeof(RadialMenu))]
    [RequireComponent(typeof(MenuMover))]
    public class MenuScaler : MonoBehaviour
    {
        [SerializeField, Range(1.1f, 2f), OnValueChanged("OnScaleFactorChanged")] float _upscaleSelectedFactor = 1.25f;
        [SerializeField, OnValueChanged("OnScaleFactorChanged"), Range(0f, 1f)]
        float _itemScaleFactor = 1f;
        [SerializeField, ReadOnly]
        float _itemUniformScale = 0f;
        public Vector3 ItemsInitialScale { get; private set; } = Vector3.zero;
        public float UniformScale => _itemUniformScale;
        public RadialMenu Menu
        {
            get
            {
                if (_menu == null) _menu = GetComponent<RadialMenu>();
                return _menu;
            }
        }
        private RadialMenu _menu;
        private MenuMover _mover;
        private Coroutine _scaleCoroutine;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            _mover = GetComponent<MenuMover>();
            Menu.OnToggleVisibility -= ToggleByScaling;
            Menu.OnToggleVisibility += ToggleByScaling;
            Menu.OnStep -= ScaleWhenRotating;
            Menu.OnStep += ScaleWhenRotating;
            Menu.OnMenuRebuild -= ScaleWhenRotating;
            Menu.OnMenuRebuild += ScaleWhenRotating;
        }

        public void CalculateScale()
        {
            if (Menu.ItemList.Count < 2) return;
            if (Menu.RadiusChangesScale)
            {
                _itemUniformScale = Menu.ItemDistance;
            }
            ItemsInitialScale = _itemUniformScale * _itemScaleFactor * Vector3.one;
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
                for (int i = 0; i < Menu.ItemList.Count; i++)
                {
                    Menu.ItemList[i].Icon.transform.localScale = ItemsInitialScale;
                }
                Menu.ItemList[Menu.ChosenIndex].Icon.transform.localScale = ItemsInitialScale * _upscaleSelectedFactor;
                Menu.SetIndicatorPositionAndScele();
            }
            else
            {
                StopAllCoroutines();
                _scaleCoroutine = StartCoroutine(ScaleGraduallyRoutine());
            }
        }

        private IEnumerator ScaleGraduallyRoutine()
        {
            Vector3[] startScales = Menu.ItemList.ConvertAll(i => i.Icon.transform.localScale).ToArray();
            float t = 0f;
            yield return null;
            // while (_mover.IsMoving)
            while (t != 1f)
            {
                t = _mover.GetInterpolator();
                for (int i = 0; i < Menu.ItemList.Count; i++)
                {
                    Vector3 newScale = Vector3.Lerp(startScales[i], GetTargetScale(i), t);
                    Menu.ItemList[i].Icon.transform.localScale = newScale;
                }
                yield return null;
            }
        }

        private IEnumerator ToggleVisibilityRoutine()
        {
            // toggle scale (between either "0" or "normal initial")           
            float a = Menu.ItemList[0].Icon.transform.localPosition.y; // start
            float b = Menu.Active ? Menu.Radius : 0.0001f; //end
            float currentRadius = 0f;
            float t = 0f;
            Vector3[] fromScale = Menu.ItemList.ConvertAll(i => i.Icon.transform.localScale).ToArray();
            while (t != 1f)
            {
                currentRadius = Menu.ItemList[0].Icon.transform.localPosition.y;
                t = Mathf.InverseLerp(a, b, currentRadius);
                for (int i = 0; i < Menu.ItemList.Count; i++)
                {
                    Vector3 newScale = Vector3.Lerp(fromScale[i], GetTargetScale(i), t);
                    Menu.ItemList[i].Icon.transform.localScale = newScale;
                }
                yield return null;
            }
        }
        Vector3 GetTargetScale(int i)
        {
            if (!Menu.Active) return Vector3.zero;

            if (i == Menu.ChosenIndex)
                return ItemsInitialScale * _upscaleSelectedFactor;
            return ItemsInitialScale;// * _downscaleFactor;
        }
        public void OnScaleFactorChanged()
        {
            if (!Menu.Initialized) Menu.Init();
            CalculateScale();
            ScaleWhenRotating();
        }
    }
}
