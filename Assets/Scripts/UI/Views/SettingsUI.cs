using System;
using BlackboxSystem;
using Infrastructure;
using Sound;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsUI : MonoBehaviour,
        IStandaloneInitializable,
        IStandaloneUpdatable,
        IEnablable,
        IInputController,
        IInputControllable,
        IInjectable<GameContext>,
        IInjectable<SfxPlayManager>,
        IInjectable<DarkscreenUI>
    {
        [field: SerializeField] public KeyCode OpenKey { get; set; } = KeyCode.Escape;
        [field: SerializeField] public bool AllowKeyOnlyWhenOpen { get; set; } = false;
        [Space]
        [SerializeField] private Animation _animation;
        [Header("Controllers")]
        [SerializeField] private Scrollbar _bgmScroll;
        [SerializeField] private Scrollbar _sfxScroll;
        [SerializeField] private Button _continueBtn;
        [SerializeField] private Button _restartBtn;
        [SerializeField] private Button _guideBtn;
        [SerializeField] private Button _relicsBtn;
        [SerializeField] private Button _exitBtn;

        public bool AllowInput { get; set; } = true;
        public event Action OpenRelicsUI;
        public event Action Destroying;

        private GameContext _gameContext;
        private SfxPlayManager _sfxPlayManager;
        private DarkscreenUI _darkscreenUI;
        private EnableWithAnimation _enabler;
        private IInputHub _inputHub;

        private const SfxName SampleSfxSound = SfxName.Click;
        private const float SampleSoundPlayGap = 0.05f;
        private float _lastSamplePlayTime;

        private bool _isInitialized = false;

        #region Interfaces
        Action IEnablable.OnEnabling => () =>
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Enabling");
            _lastSamplePlayTime = float.MinValue;

            if (_darkscreenUI)
            {
                BlackboxHandle.Of(this).Exert(_darkscreenUI, $"Enable Darkscreen");
                _darkscreenUI.EnableFor(this, Disable);
            }

            if (_inputHub != null)
            {
                _bgmScroll.value = _gameContext.BgmVolume / 100f;
                _sfxScroll.value = _gameContext.SfxVolume / 100f;

                BlackboxHandle.Of(this).Exert(_inputHub, $"BlockAll InputHub'");
                _inputHub.BlockExcept(this);
            }
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () =>
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Disabling");

            if (_darkscreenUI)
            {
                BlackboxHandle.Of(this).Exert(_darkscreenUI, $"Disable from '{name}'");
                _darkscreenUI.Disable();
            }

            if (_inputHub != null)
            {
                BlackboxHandle.Of(this).Exert(_inputHub, $"UnblockAll from '{name}'");
                _inputHub.UnblockAll();
            }
        };
        Action IEnablable.OnDisabled => null;
        #endregion


        private void Awake() => ((IStandaloneInitializable)this).StandaloneInitialize();
        void IStandaloneInitializable.StandaloneInitialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Initialize");

            if (!_animation)
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_animation)} 컴포넌트가 유효하지 않습니다.")));

            if (!_bgmScroll)
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(_bgmScroll)} 컴포넌트가 유효하지 않습니다.");
            }
            else
                _bgmScroll.onValueChanged.AddListener(val =>
                {
                    var vol = Mathf.RoundToInt(val * 100);
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_bgmScroll, $"Set Bgm Vol: {vol}");

                    _gameContext.SetBgmVolume(vol, this);
                });

            if (!_sfxScroll)
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(_sfxScroll)} 컴포넌트가 유효하지 않습니다.");
            }
            else
                _sfxScroll.onValueChanged.AddListener(val =>
                {
                    var vol = Mathf.RoundToInt(val * 100);
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_sfxScroll, $"Set Sfx Vol: {vol}");

                    _gameContext.SetSfxVolume(vol, this);

                    if (_sfxPlayManager)
                    {
                        var currentTime = Time.unscaledTime;
                        if (currentTime - _lastSamplePlayTime >= SampleSoundPlayGap)
                        {
                            _lastSamplePlayTime = currentTime;
                            _sfxPlayManager.Play(SampleSfxSound);
                        }
                    }
                    else
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                            $"{nameof(_sfxPlayManager)}이(가) 유효하지 않기 때문에 샘플 사운드를 재생할 수 없습니다."),
                            this);
                });

            if (!_continueBtn)
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(_continueBtn)} 컴포넌트가 유효하지 않습니다.");
            }
            else
                _continueBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_continueBtn, "Continue");
                    ((IEnablable)this).Disable();
                });

            if (!_restartBtn)
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(_restartBtn)} 컴포넌트가 유효하지 않습니다.");
            }
            else
                _restartBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_restartBtn, "Restart");
                    print("재시작");
                });

            if (!_guideBtn)
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(_guideBtn)} 컴포넌트가 유효하지 않습니다.");
            }
            else
                _guideBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_guideBtn, "Show Guide");
                    print("가이드 열기");
                });

            if (!_relicsBtn)
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(_relicsBtn)} 컴포넌트가 유효하지 않습니다.");
            }
            else
                _relicsBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_relicsBtn, "Show Relics");
                    OnOpenRelicsUI();
                });

            if (!_exitBtn)
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(_exitBtn)} 컴포넌트가 유효하지 않습니다.");
            }
            else
                _exitBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_exitBtn, "게임 종료");

                    if(_gameContext)
                        _gameContext.Quit(this);
                    else
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                            "GameContext가 유효하지 않기 때문에 게임을 종료할 수 없습니다.")), this);
                });


            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            ((IEnablable)this).SetToDisabled();
        }

        void IInjectable<GameContext>.Inject(GameContext gameContext) => _gameContext = gameContext;
        void IInjectable<SfxPlayManager>.Inject(SfxPlayManager sfxPalyManager) => _sfxPlayManager = sfxPalyManager;
        void IInjectable<DarkscreenUI>.Inject(DarkscreenUI darkscreenUI) => _darkscreenUI = darkscreenUI;
        void IInputController.Initialize(IInputHub inputHub) => _inputHub = inputHub;

        void IStandaloneUpdatable.StandaloneUpdate()
        {
            if (!AllowInput)
                return;

            if (Input.GetKeyDown(OpenKey))
            {
                if (!_enabler.IsEnabled)
                {
                    if (!AllowKeyOnlyWhenOpen)
                        Enable();
                }
                else
                    Disable();
            }
        }

        private void OnOpenRelicsUI()
        {
            if (OpenRelicsUI == null)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                    $"{nameof(OpenRelicsUI)} 이벤트에 등록된 대리자가 없으므로 유물 UI를 열 수 없습니다.")),
                    this);
                return;
            }

            Disable();
            OpenRelicsUI.Invoke();
        }

        public void Enable() => _enabler.Enable();
        public void Disable() => _enabler.Disable();
        public void SetToEnabled() => _enabler.SetToEnabled();
        public void SetToDisabled() => _enabler.SetToDisabled();

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");

            Destroying?.Invoke();
            _enabler?.Dispose();
        }

        private string Ctx(string message) => $"[{nameof(SettingsUI)}] {message}";
    }
}
