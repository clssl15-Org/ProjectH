using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    [RequireComponent(typeof(RectTransform), typeof(Slider))]
    public class HealthBar : MonoBehaviour, IView
    {
        public event Action Destroyed;
        [SerializeField] Vector2 _localPosition = Vector2.zero;

        private RectTransform _transform;
        private Slider _slider;
        private IHealthRateVM _vm;


        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
            _slider = GetComponent<Slider>();
        }

        public void Connect(IHealthRateVM vm)
        {
            if (_vm == vm)
                return;
            if (_vm != null)
                throw new InvalidOperationException(
                    $"{nameof(IHealthRateVM)}이(가) 이미 존재하기 때문에 새로운 연결을 구성할 수 없습니다.");

            _vm = vm;
            _vm.HealthRateChanged += SetHealthRate;
            _vm.Disposed += Destroy;

            SetHealthRate(_vm.HealthRate);
        }

        public void Disconnect()
        {
            if (_vm == null)
                return;

            _vm.HealthRateChanged -= SetHealthRate;
            _vm.Disposed -= Destroy;

            _vm = null;
        }

        private void Update()
        {
            if (_vm == null)
                return;

            _transform.position
                = Camera.main.WorldToScreenPoint(_vm.WorldPosition)
                + (Vector3)_localPosition;
        }

        private void SetHealthRate(HealthRateData data)
        {
            _slider.value = data.HealthRate;
        }

        public void SetParent(RectTransform parent) => _transform.SetParent(parent);

        public void Destroy()
        {
            Destroyed?.Invoke();
            Destroyed = null;

            Disconnect();
            Destroy(gameObject);
        }
        private void OnDestroy() => Disconnect();
    }
}
