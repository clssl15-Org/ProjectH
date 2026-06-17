using System;
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

            _work = new Work()
                .AddChild(new Work(State.Idle)
                    .OnEntered(() => _effect.enabled = false)
                    .OnUpdated(() =>
                    {
                        var rate = Mathf.Clamp01(_getGaugeRate());
                        SetGauge(rate);

                        if (rate >= 1f)
                        {
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
            _getGaugeRate = getGaugeRate;
        }

        private void Start()
        {
            _work?.Enter();
        }

        private void SetGauge(float targetAmount)
        {
            // ���� Ÿ�̸Ӱ� ���� �ִٸ� ���
            _gaugeTimer?.Dispose();

            float startAmount = _gauge.fillAmount;
            float elapsedTime = 0f;

            if (Mathf.Approximately(startAmount, targetAmount))
                return;

            IDisposable timer = null;
            timer = _gaugeTimer = Loco.Subscribe(() =>
            {
                // _gaugeDealyRate�� 0�̰ų� ������ ��� 0���� ������ ���� �� ��� ����
                if (_gaugeDealyRate <= 0f)
                {
                    _gauge.fillAmount = targetAmount;
                    timer?.Dispose();
                    return;
                }

                elapsedTime += Time.unscaledTime;

                // 0.0 ~ 1.0 ������ �ð� ���൵
                float t = Mathf.Clamp01(elapsedTime / _gaugeDealyRate);

                // �������� �ټ��� ���� Ease Out Cubic � ����
                // t�� 1�� ����������� ��ȭ���� �پ��� �ε巴�� �����մϴ�.
                float easeT = 1f - Mathf.Pow(1f - t, 3);

                _gauge.fillAmount = Mathf.Lerp(startAmount, targetAmount, easeT);

                // ������ �Ϸ�Ǿ��� ��
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
