using System;
using BlackboxSystem;
using Infrastructure;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerView
{
    public class UltimateUI : MonoBehaviour
    {
        [SerializeField] private Image _gauge;
        [SerializeField] private Image _effect;
        [SerializeField] private Animation _effectBlinkAnimation;
        [SerializeField, Min(0)] private float _gaugeDealyRate = 0.5f;
        [Space]
        [SerializeField] private string _display;

        private enum State
        {
            Idle,
            Charged,
        }
        private Work _work;

        private Func<float> _getGaugeRate;
        private IDisposable _gaugeTimer;


        // Content
        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            _work = new Work()
                .AddChild(new Work(State.Idle)
                    .OnEntered(() => _effect.enabled = false)
                    .OnUpdated(() =>
                    {
                        var rate = Mathf.Clamp01(_getGaugeRate());
                        SetGauge(rate);

                        if (rate >= 1f)
                        {
                            using var _ = BlackboxHandle.Of(this).WriteScope("Gauge reached 100%, transitioning to Charged state.");
                            _work.SetNext(State.Charged);
                        }
                    }),
                    isPrimary: true
                )
                .AddChild(new Work(State.Charged)
                    .OnEntered(() =>
                    { 
                        SetGauge(1f);

                        _effect.enabled = true;
                        _effectBlinkAnimation[_effectBlinkAnimation.clip.name].time = 0f;
                        _effectBlinkAnimation.Play();
                    })
                    .OnUpdated(() =>
                    {
                        if (_getGaugeRate() < 1f)
                        {
                            using var _ = BlackboxHandle.Of(this).WriteScope("Gauge dropped below 100%, returning to Idle state.");
                            _work.SetNext(State.Idle);
                        }
                    })
                    .OnExited(() =>
                    {
                        _effect.enabled = false;
                        _effectBlinkAnimation.Stop();
                    })
                );
        }

        public void Initialize(Func<float> getGaugeRate)
        {
            BlackboxHandle.Of(this).WriteScope("Initialize");
            _getGaugeRate = getGaugeRate;
        }

        private void Start()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Start");
            _work?.Enter();
        }

        private void SetGauge(float targetAmount)
        {
            // 기존 타이머가 돌고 있다면 취소
            _gaugeTimer?.Dispose();

            float startAmount = _gauge.fillAmount;
            float elapsedTime = 0f;

            if (Mathf.Approximately(startAmount, targetAmount))
                return;

            IDisposable timer = null;
            timer = _gaugeTimer = Loco.Subscribe(() =>
            {
                // _gaugeDealyRate가 0이거나 음수일 경우 0으로 나누기 방지 및 즉시 적용
                if (_gaugeDealyRate <= 0f)
                {
                    _gauge.fillAmount = targetAmount;
                    timer?.Dispose();
                    return;
                }

                elapsedTime += Time.unscaledTime;

                // 0.0 ~ 1.0 사이의 시간 진행도
                float t = Mathf.Clamp01(elapsedTime / _gaugeDealyRate);

                // 감각적인 텐션을 위한 Ease Out Cubic 곡선 적용
                // t가 1에 가까워질수록 변화량이 줄어들어 부드럽게 안착합니다.
                float easeT = 1f - Mathf.Pow(1f - t, 3);

                _gauge.fillAmount = Mathf.Lerp(startAmount, targetAmount, easeT);

                // 진행이 완료되었을 때
                if (t >= 1f)
                {
                    _gauge.fillAmount = targetAmount;
                    timer?.Dispose();
                    return;
                }
            });
        }

        private void Update()
        {
            _work?.Update();

# if UNITY_EDITOR
            if (_work != null)
            {
                if (_work.TryGetCurrentChild(out var child))
                    _display = $"Current: {child.Name}";
                else
                    _display = "Current is null";
            }
#endif
        }

        private void OnDestroy()
        {
            BlackboxHandle.Of(this).WriteScope("Destroy");

            if (_work != null)
            {
                _work.Dispose();
                _work = null;
            }

            _gaugeTimer?.Dispose();
            _getGaugeRate = null;
        }
    }
}
