using System;
using BlackboxSystem;
using Infrastructure.StateMachines.Fsm;
using Unity.VisualScripting;
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
        private Func<float> _getProgress;


        // Content
        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            _work = new Work()
                .AddChild(new Work(State.Idle)
                    .OnEntered(() => SetEnable(false)),
                    isPrimary: true
                )
                .AddChild(new Work(State.Cooltime)
                    .OnEntered(() => SetEnable(true))
                    .OnUpdated(() =>
                    {
                        if (_getProgress == null)
                        {
                            Debug.LogWarning(BlackboxHandle.Of(this).WriteError(
                                $"[SkillCooltimeManager] {nameof(_getProgress)} 이벤트가 null이기 때문에 " +
                                $"{nameof(State.Cooltime)} 모드로 진입할 수 없습니다."));

                            _image.fillAmount = 0f;
                            _work.SetNextToNone();
                            return;
                        }

                        var progress = _getProgress();
                        if (progress >= 1f)
                        {
                            _image.fillAmount = 1f;
                            _work.SetNext(State.Idle);
                            return;
                        }

                        _image.fillAmount = 1 - progress;
                    })
                    .OnExited(() => _getProgress = null)
                );

            SetEnable(false);
        }

        private void Start()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Start");
            _work?.Enter();
        }

        public void Enable(Func<float> getProgress)
        {
            _getProgress = getProgress;
            _work?.SetNext(State.Cooltime);
        }
        public void Disable() => _work?.SetNext(State.Idle);

        private void SetEnable(bool enable)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Enable, '{_isEnabled}' -> '{enable}'");

            if (_isEnabled == enable) return;
            _isEnabled = enable;

            _enableTimer?.Dispose();
            _enableTimer = null;

            if (!_image.enabled && _image.color.a > 0f)
                _image.color = _image.color.WithAlpha(0f);

            if (_enableTime <= 0f || _defaultAlpha <= 0f)
            {
                _image.color = _image.color.WithAlpha(enable ? _defaultAlpha : 0f);
                _image.enabled = enable;
                return;
            }

            var progress = Mathf.Clamp01(_image.color.a / _defaultAlpha);
            if (!enable) progress = 1f - progress;

            bool UpdateImage()
            {
                progress = Mathf.Clamp01(progress + Time.deltaTime / _enableTime);
                _image.color = _image.color.WithAlpha(_defaultAlpha * (enable ? progress : 1f - progress));

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
                time: _enableTime * (1f - progress),
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
            BlackboxHandle.Of(this).WriteScope("Destroy");

            _enableTimer?.Dispose();

            if (_work != null)
            {
                _work.Dispose();
                _work = null;
            }
        }
    }
}
