using System;
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
            Time.timeScale = 0f;

            _lastSamplePlayTime = Time.unscaledTime;
            _bgmScroll.value = _gameServices.BgmVolume / 100f;
            _sfxScroll.value = _gameServices.SfxVolume / 100f;

            transform.SetAsLastSibling();
            _cursorVisibilityController?.RequestVisible(this);

            if (_inputHub != null)
            {
                _inputHub.Block(this);
            }

            if (_darkscreenUI)
            {
                _darkscreenUI.EnableFor(this, Disable);
            }
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () =>
        {
            Time.timeScale = 1f;
            _cursorVisibilityController?.ReleaseVisible(this);

            if (_darkscreenUI)
            {
                _darkscreenUI.Disable();
            }

            if (_inputHub != null)
            {
                _inputHub.Unblock(this);
            }
        };
        Action IEnablable.OnDisabled => null;
        #endregion


        private void Awake() => ((IStandaloneInitializable)this).StandaloneInitialize();
        void IStandaloneInitializable.StandaloneInitialize()
        {

            if (_isInitialized) return;
            _isInitialized = true;

            if (!_animation)
                throw new InvalidOperationException(Ctx($"{nameof(_animation)} ������Ʈ�� ��ȿ���� �ʽ��ϴ�."));

            if (!_bgmScroll)
            {
            }
            else
                _bgmScroll.onValueChanged.AddListener(val =>
                {
                    var vol = Mathf.RoundToInt(val * 100);

                    _gameServices.SetBgmVolume(vol, this);
                });

            if (!_sfxScroll)
            {
            }
            else
                _sfxScroll.onValueChanged.AddListener(val =>
                {
                    var vol = Mathf.RoundToInt(val * 100);

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
                        Debug.LogWarning($"{nameof(_sfxPlayManager)}��(��) ��ȿ���� �ʱ� ������ ���� ���带 ����� �� �����ϴ�.",
                            this);
                });

            if (!_continueBtn)
            {
            }
            else
                _continueBtn.onClick.AddListener(() =>
                {
                    ((IEnablable)this).Disable();
                });

            if (!_restartBtn)
            {
            }
            else
                _restartBtn.onClick.AddListener(() =>
                {
                    OnRestart();
                });

            if (!_guideBtn)
            {
            }
            else
                _guideBtn.onClick.AddListener(() =>
                {
                    OnOpenGuideUI();
                });

            if (!_relicsBtn)
            {
            }
            else
                _relicsBtn.onClick.AddListener(() =>
                {
                    OnOpenRelicsUI();
                });

            if (!_exitBtn)
            {
            }
            else
                _exitBtn.onClick.AddListener(() =>
                {

                    if (_gameServices)
                    {
                        Disable();
                        _gameServices.ChangeScene("Title", this, true);
                    }
                    else
                        Debug.LogWarning(Ctx(
                            "gameServices�� ��ȿ���� �ʱ� ������ ������ ������ �� �����ϴ�."), this);
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
                Debug.LogWarning(Ctx(
                    $"{nameof(RestartUI)} �̺�Ʈ�� ��ϵ� �븮�ڰ� �����Ƿ� ���̵� UI�� �� �� �����ϴ�."),
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
                Debug.LogWarning(Ctx(
                    $"{nameof(OpenGuideUI)} �̺�Ʈ�� ��ϵ� �븮�ڰ� �����Ƿ� ���̵� UI�� �� �� �����ϴ�."),
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
                Debug.LogWarning(Ctx(
                    $"{nameof(OpenRelicsUI)} �̺�Ʈ�� ��ϵ� �븮�ڰ� �����Ƿ� ���� UI�� �� �� �����ϴ�."),
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

            _cursorVisibilityController?.ReleaseVisible(this);
            Destroying?.Invoke();
            _openerSubject?.Dispose();
            _enabler?.Dispose();
        }

        private string Ctx(string message) => $"[{nameof(SettingsUI)}] {message}";
    }
}
