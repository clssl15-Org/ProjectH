using System;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerView
{
    public class SkillCooltimeManager : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField, Range(0, 1)] private float _defaultAlpha = 0.5f;
        [SerializeField, Min(0)] private float _enableTime = 0.2f;
        [Space]
        [SerializeField] private string _display;

        private enum State
        {
            Idle,
            Cooltime,
        }
        private Work _work;

        private bool? _isEnabled = null;
        private IDisposable _enableTimer;
        private Func<float> _getCooltimeRate;


        // Content
        private void Awake()
        {

            _work = new Work()
                .AddChild(new Work(State.Idle)
                    .OnEntered(() => SetEnable(false)),
                    isPrimary: true
                )
                .AddChild(new Work(State.Cooltime)
                    .OnEntered(() => SetEnable(true))
                    .OnUpdated(() =>
                    {
                        if (_getCooltimeRate == null)
                        {
                            Debug.LogWarning($"[SkillCooltimeManager] {nameof(_getCooltimeRate)} �̺�Ʈ�� null�̱� ������ " +
                                $"{nameof(State.Cooltime)} ���� ������ �� �����ϴ�.");

                            _image.fillAmount = 0f;
                            _work.SetNextToNone();
                            return;
                        }

                        var progress = _getCooltimeRate();
                        if (progress < 0f)
                        {
                            _image.fillAmount = 0f;
                            _work.SetNext(State.Idle);
                            return;
                        }

                        _image.fillAmount = progress;
                    })
                    .OnExited(() => _getCooltimeRate = null)
                );

            SetEnable(false);
        }

        private void Start()
        {
            _work?.Enter();
        }

        public void Enable(Func<float> getCooltime)
        {
            _getCooltimeRate = getCooltime;
            _work?.SetNext(State.Cooltime);
        }
        public void Disable() => _work?.SetNext(State.Idle);

        private void SetEnable(bool enable)
        {

            if (_isEnabled == enable) return;
            _isEnabled = enable;

            _enableTimer?.Dispose();
            _enableTimer = null;

            if (!_image.enabled && _image.color.a > 0f)
            {
                var c = new Color(_image.color.r, _image.color.g, _image.color.b, 0f);
                _image.color = c;
            }

            if (_enableTime <= 0f || _defaultAlpha <= 0f)
            {
                var c = new Color(
                    _image.color.r,
                    _image.color.g,
                    _image.color.b,
                    enable ? _defaultAlpha : 0f);

                _image.color = c;
                _image.enabled = enable;
                return;
            }

            var progress = Mathf.Clamp01(_image.color.a / _defaultAlpha);
            if (!enable) progress = 1f - progress;

            bool UpdateImage()
            {
                progress = Mathf.Clamp01(progress + Time.deltaTime / _enableTime);

                var c = new Color(
                    _image.color.r,
                    _image.color.g,
                    _image.color.b,
                    _defaultAlpha * (enable ? progress : 1f - progress));
                _image.color = c;

                if (progress >= 1f)
                {
                    _image.enabled = enable;
                    return false;
                }

                return true;
            }

            if (!UpdateImage()) return;
            _image.enabled = true;

            _enableTimer = new Infrastructure.Timer(
                time:    _enableTime * (1f - progress),
                updated: () => UpdateImage());
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

            _enableTimer?.Dispose();

            if (_work != null)
            {
                _work.Dispose();
                _work = null;
            }
        }
    }
}
