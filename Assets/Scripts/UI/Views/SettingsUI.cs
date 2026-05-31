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
        IInputLayerController,
        ICursorVisibilityControllerUser,
        IInjectable<GameServices>,
        IInjectable<SfxPlayManager>,
        IInjectable<DarkscreenUI>
    {
        [field: SerializeField] public KeyCode OpenKey { get; set; } = KeyCode.Escape;
        [field: SerializeField] public KeyCode CloseKey { get; set; } = KeyCode.Escape;
        [Space]
        [SerializeField] private Animation _animation;
        [SerializeField] private SfxName _sampleSfxSound = SfxName.Hover;
        [Header("Controllers")]
        [SerializeField] private Scrollbar _bgmScroll;
        [SerializeField] private Scrollbar _sfxScroll;
        [SerializeField] private Button _continueBtn;
        [SerializeField] private Button _restartBtn;
        [SerializeField] private Button _guideBtn;
        [SerializeField] private Button _relicsBtn;
        [SerializeField] private Button _exitBtn;

        public event Action RestartUI;
        public event Action OpenRelicsUI;
        public event Action OpenGuideUI;
        public event Action Destroying;

        private GameServices _gameServices;
        private SfxPlayManager _sfxPlayManager;
        private DarkscreenUI _darkscreenUI;
        private EnableWithAnimation _enabler;

        private const float SampleSoundPlayGap = 0.05f;
        private float _lastSamplePlayTime;

        private IInputHub _inputHub;
        private CursorVisibilityController _cursorVisibilityController;
        private EmptyInputSubject _openerSubject;
        private bool _isInitialized = false;

        #region Interfaces
        Action IEnablable.OnEnabling => () =>
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Enabling");
            Time.timeScale = 0f;

            _lastSamplePlayTime = Time.unscaledTime;
            _bgmScroll.value = _gameServices.BgmVolume / 100f;
            _sfxScroll.value = _gameServices.SfxVolume / 100f;

            transform.SetAsLastSibling();
            _cursorVisibilityController?.RequestVisible(this);

            if (_inputHub != null)
            {
                BlackboxHandle.Of(this).Exert(_inputHub, "Block");
                _inputHub.Block(this);
            }

            if (_darkscreenUI)
            {
                BlackboxHandle.Of(this).Exert(_darkscreenUI, "Enable Darkscreen");
                _darkscreenUI.EnableFor(this, Disable);
            }
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () =>
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Disabling");
            Time.timeScale = 1f;
            _cursorVisibilityController?.ReleaseVisible(this);

            if (_darkscreenUI)
            {
                BlackboxHandle.Of(this).Exert(_darkscreenUI, "Disable Darkscreen");
                _darkscreenUI.Disable();
            }

            if (_inputHub != null)
            {
                BlackboxHandle.Of(this).Exert(_inputHub, "Unblock");
                _inputHub.Unblock(this);
            }
        };
        Action IEnablable.OnDisabled => null;
        #endregion


        private void Awake() => ((IStandaloneInitializable)this).StandaloneInitialize();
        void IStandaloneInitializable.StandaloneInitialize()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Initialize, wasInitialized: {_isInitialized}");

            if (_isInitialized) return;
            _isInitialized = true;

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

                    _gameServices.SetBgmVolume(vol, this);
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

                    _gameServices.SetSfxVolume(vol, this);

                    if (_sfxPlayManager)
                    {
                        var currentTime = Time.unscaledTime;
                        if (currentTime - _lastSamplePlayTime >= SampleSoundPlayGap)
                        {
                            _lastSamplePlayTime = currentTime;
                            _sfxPlayManager.Play(_sampleSfxSound);
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
                    OnRestart();
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
                    OnOpenGuideUI();
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

                    if (_gameServices)
                    {
                        Disable();
                        _gameServices.ChangeScene("Title", this, true);
                    }
                    else
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                            "gameServices가 유효하지 않기 때문에 게임을 종료할 수 없습니다.")), this);
                });


            _openerSubject = new EmptyInputSubject(nameof(SettingsUI), true);

            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            ((IEnablable)this).SetToDisabled();
        }

        public EmptyInputSubject GetOpenerSubject()
        {
            ((IStandaloneInitializable)this).StandaloneInitialize();
            return _openerSubject;
        }
        void IInputLayerController.Initialize(IInputHub inputHub) => _inputHub = inputHub;
        void ICursorVisibilityControllerUser.Initialize(CursorVisibilityController cursorVisibilityController) =>
            _cursorVisibilityController = cursorVisibilityController;

        void IInjectable<GameServices>.Inject(GameServices gameServices) => _gameServices = gameServices;
        void IInjectable<SfxPlayManager>.Inject(SfxPlayManager sfxPalyManager) => _sfxPlayManager = sfxPalyManager;
        void IInjectable<DarkscreenUI>.Inject(DarkscreenUI darkscreenUI) => _darkscreenUI = darkscreenUI;

        void IStandaloneUpdatable.StandaloneUpdate()
        {
            if (!_enabler.IsEnabled)
            {
                if (_openerSubject.AllowInput && Input.GetKeyDown(OpenKey))
                    Enable();
            }
            else
            {
                if (Input.GetKeyDown(CloseKey))
                    Disable();
            }
        }

        private void OnRestart()
        {
            if (RestartUI == null)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                    $"{nameof(RestartUI)} 이벤트에 등록된 대리자가 없으므로 가이드 UI를 열 수 없습니다.")),
                    this);
                return;
            }

            Disable();
            RestartUI.Invoke();
        }
        private void OnOpenGuideUI()
        {
            if (OpenGuideUI == null)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                    $"{nameof(OpenGuideUI)} 이벤트에 등록된 대리자가 없으므로 가이드 UI를 열 수 없습니다.")),
                    this);
                return;
            }

            Disable();
            OpenGuideUI.Invoke();
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

            _cursorVisibilityController?.ReleaseVisible(this);
            Destroying?.Invoke();
            _openerSubject?.Dispose();
            _enabler?.Dispose();
        }

        private string Ctx(string message) => $"[{nameof(SettingsUI)}] {message}";
    }
}
