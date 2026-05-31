using System;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class HealthBarUI : MonoBehaviour, IView
    {
        public event Action Destroying;

        protected RectTransform Transform { get; private set; }
        protected IHealthRateVM HealthRateVM { get; private set; }

        [SerializeField] private RectTransform _mask;
        [SerializeField] private RectTransform _gauge;
        [SerializeField, Min(0f)] private float _minMaskWidth = 0f;
        [SerializeField, Min(0f)] private float _maxMaskWidth = 0f;
        private float _originalWidth;
        private float _originalGaugeWidth;
        private bool _awaken;


        protected virtual void Awake()
        {
            if (_awaken) return;
            _awaken = true;

            if (!_mask)
                throw new InvalidOperationException(
                    $"{nameof(_mask)} 필드는 null일 수 없습니다. " +
                    $"인스펙터에서 올바르게 설정되었는지 확인하세요.");

            Transform = GetComponent<RectTransform>();
            _originalWidth = Transform.rect.width;
            _originalGaugeWidth = _gauge ? _gauge.rect.width : 0f;
        }

        public void Connect(IHealthRateVM vm)
        {
            Awake();

            if (vm == null)
                throw new ArgumentNullException(
                    nameof(vm),
                    $"[{nameof(HealthBarUI)}] 인자는 null일 수 없습니다.");
            if (HealthRateVM == vm)
                return;
            if (HealthRateVM != null)
                throw new InvalidOperationException(
                    $"[{nameof(HealthBarUI)}] {nameof(HealthRateVM)}이(가) 이미 존재하기 때문에 새로운 연결을 구성할 수 없습니다.");

            HealthRateVM = vm;
            HealthRateVM.HealthRateChanged += SetHealthRate;
            HealthRateVM.Disposed += Destroy;

            SetHealthRate(HealthRateVM.HealthRate);
            return;
        }

        public void Disconnect()
        {
            if (HealthRateVM == null)
                return;

            HealthRateVM.HealthRateChanged -= SetHealthRate;
            HealthRateVM.Disposed -= Destroy;

            HealthRateVM = null;
            Transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _originalWidth);
            if (_gauge)
                _gauge.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _originalGaugeWidth);
        }

        private void SetHealthRate(HealthRateData data)
        {
            var baseMaxHealth = Mathf.Max(data.BaseMaxHealth, 1f);
            var width = _originalWidth * Mathf.Max(data.MaxHealth / baseMaxHealth, 0f);
            Transform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width);
            if (_gauge)
                _gauge.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    GetGaugeWidth(width));
            _mask.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                GetMaskWidth(data.HealthRate, width));
        }

        private float GetGaugeWidth(float width)
        {
            var widthScale = _originalWidth > 0f ? width / _originalWidth : 1f;
            return _originalGaugeWidth * widthScale;
        }


        private float GetMaskWidth(float healthRate, float width)
        {
            var widthScale = _originalWidth > 0f ? width / _originalWidth : 1f;
            var minMaskWidth = _minMaskWidth * widthScale;
            var maxMaskWidth = (_maxMaskWidth > 0f
                    ? Mathf.Max(_maxMaskWidth, _minMaskWidth)
                    : _originalWidth)
                * widthScale;

            minMaskWidth = Mathf.Clamp(minMaskWidth, 0f, width);
            maxMaskWidth = Mathf.Clamp(maxMaskWidth, minMaskWidth, width);

            return Mathf.Lerp(minMaskWidth, maxMaskWidth, Mathf.Clamp01(healthRate));
        }

        public void SetParent(RectTransform parent) => Transform.SetParent(parent);

        public void Destroy()
        {
            Destroying?.Invoke();
            Destroying = null;

            Disconnect();
            if (this && gameObject) Destroy(gameObject);
        }
        private void OnDestroy() => Disconnect();
    }
}
