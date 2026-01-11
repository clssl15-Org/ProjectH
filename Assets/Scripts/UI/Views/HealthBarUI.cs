using System;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class HealthBarUI : MonoBehaviour, IView
    {
        public event Action Destroyed;

        protected RectTransform Transform { get; private set; }
        protected IHealthRateVM HealthRateVM { get; private set; }

        [SerializeField] private RectTransform _mask;
        private float _originalWidth;

        private bool _awaken = false;


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
        }

        private void SetHealthRate(HealthRateData data)
        {
            _mask.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                data.HealthRate * _originalWidth);
        }

        public void SetParent(RectTransform parent) => Transform.SetParent(parent);

        public void Destroy()
        {
            Destroyed?.Invoke();
            Destroyed = null;

            Disconnect();
            if (this && gameObject) Destroy(gameObject);
        }
        private void OnDestroy() => Disconnect();
    }
}
