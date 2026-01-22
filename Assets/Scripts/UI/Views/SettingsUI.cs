using System;
using BlackboxSystem;
using Infrastructure;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
    public class SettingsUI : MonoBehaviour,
        IStandaloneInitializable,
        IStandaloneUpdatable,
        IEnablable,
        IInputController,
        IInputControllable,
        IInjectable<GameContext>
    {
        [field: SerializeField] public KeyCode OpenKey { get; set; } = KeyCode.Escape;
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
        public event Action Destroyed;

        private GameContext _gameContext;
        private EnableWithAnimation _enabler;
        private IInputHub _inputHub;
        private bool _isInitialized = false;

        #region interfaces
        Action IEnablable.OnEnabling => () =>
        {
            if (_inputHub != null)
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Enabling");

                _bgmScroll.value = _gameContext.BgmVolume / 100f;
                _sfxScroll.value = _gameContext.SfxVolume / 100f;

                BlackboxHandle.Of(this).Exert(_inputHub, "BlockAll");
                _inputHub.BlockExcept(this);
            }
        };
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => () =>
        {
            if (_inputHub != null)
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Disabling");

                BlackboxHandle.Of(this).Exert(_inputHub, "UnblockAll");
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
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_bgmScroll)} 컴포넌트가 유효하지 않습니다.")));
            }
            else
                _bgmScroll.onValueChanged.AddListener(val =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_bgmScroll, "Set Bgm Vol");
                    _gameContext.SetBgmVolume(Mathf.RoundToInt(val * 100), this);
                });

            if (!_sfxScroll)
            {
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_sfxScroll)} 컴포넌트가 유효하지 않습니다.")));
            }
            else
                _sfxScroll.onValueChanged.AddListener(val =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_sfxScroll, "Set Sfx Vol");
                    _gameContext.SetSfxVolume(Mathf.RoundToInt(val * 100), this);
                });

            if (!_continueBtn)
            {
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_continueBtn)} 컴포넌트가 유효하지 않습니다.")));
            }
            else
                _continueBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_continueBtn, "Continue");
                    ((IEnablable)this).Disable();
                });

            if (!_restartBtn)
            {
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_restartBtn)} 컴포넌트가 유효하지 않습니다.")));
            }
            else
                _restartBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_restartBtn, "Restart");
                    print("재시작");
                });

            if (!_guideBtn)
            {
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_guideBtn)} 컴포넌트가 유효하지 않습니다.")));
            }
            else
                _guideBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_guideBtn, "Show Guide");
                    print("가이드 열기");
                });

            if (!_relicsBtn)
            {
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_relicsBtn)} 컴포넌트가 유효하지 않습니다.")));
            }
            else
                _relicsBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_relicsBtn, "Show Relics");
                    OnOpenRelicsUI();
                });

            if (!_exitBtn)
            {
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    Ctx($"{nameof(_exitBtn)} 컴포넌트가 유효하지 않습니다.")));
            }
            else
                _exitBtn.onClick.AddListener(() =>
                {
                    using var _ = BlackboxHandle.Of(this).ExertedScope(_exitBtn, "게임 종료");

                    if(_gameContext)
                        _gameContext.Quit(this);
                    else
                        Debug.LogWarning(BlackboxHandle.Of(this).Write(Ctx(
                            "GameContext가 유효하지 않기 때문에 게임을 종료할 수 없습니다.")), this);
                });


            _enabler = new EnableWithAnimation(_animation)
                .InitializeWithIEnablable(this);

            ((IEnablable)this).SetToDisabled();
        }

        void IInjectable<GameContext>.Inject(GameContext gameContext) => _gameContext = gameContext;
        void IInputController.Initialize(IInputHub inputHub) => _inputHub = inputHub;

        void IStandaloneUpdatable.StandaloneUpdate()
        {
            if (!AllowInput)
                return;

            if (Input.GetKeyDown(OpenKey))
            {
                if (!_enabler.IsEnabled)
                    Enable();
                else
                    Disable();
            }
        }

        private void OnOpenRelicsUI()
        {
            if (OpenRelicsUI == null)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).Write(Ctx(
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

            Destroyed?.Invoke();
            _enabler?.Dispose();
        }

        private string Ctx(string message) => $"[{nameof(SettingsUI)}] {message}";


#if UNITY_EDITOR
        [CustomEditor(typeof(SettingsUI))]
        private class SettingsUIEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (Application.isPlaying)
                {
                    GUILayout.Space(8);
                    if (GUILayout.Button("Export Log"))
                        BlackboxHandle.Of(target).Export();
                }
            }
        }
#endif
    }
}
