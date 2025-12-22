using System;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class HealthBar : MonoBehaviour, IView
    {
        public event Action Destroyed;

        protected RectTransform Transform { get; private set; }
        protected IHealthRateVM HeanthRateVM { get; private set; }

        [SerializeField] RectTransform _mask;
        private float _originalWidth;



        protected virtual void Awake()
        {
            if (!_mask)
                throw new InvalidOperationException(
                    $"{nameof(_mask)} 필드는 null일 수 없습니다. " +
                    $"인스펙터에서 올바르게 설정되었는지 확인하세요.");

            Transform = GetComponent<RectTransform>();
            _originalWidth = Transform.rect.width;
        }

        public void Connect(IHealthRateVM vm)
        {
            if (HeanthRateVM == vm)
                return;
            if (HeanthRateVM != null)
                throw new InvalidOperationException(
                    $"{nameof(IHealthRateVM)}이(가) 이미 존재하기 때문에 새로운 연결을 구성할 수 없습니다.");

            HeanthRateVM = vm;
            HeanthRateVM.HealthRateChanged += SetHealthRate;
            HeanthRateVM.Disposed += Destroy;

            SetHealthRate(HeanthRateVM.HealthRate);
            return;
        }

        public void Disconnect()
        {
            if (HeanthRateVM == null)
                return;

            HeanthRateVM.HealthRateChanged -= SetHealthRate;
            HeanthRateVM.Disposed -= Destroy;

            HeanthRateVM = null;
        }

        protected virtual void Update()
        {
            if (HeanthRateVM == null)
                return;
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
