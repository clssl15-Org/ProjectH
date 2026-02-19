using System;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using UI;
using BlackboxSystem;

namespace Game.Title
{
    public class Menu : MonoBehaviour,
        IInjectable<GameServices>,
        IEnablable,
        IInputControllable
    {
        public event Action Enabling;
        public event Action Disabling;
        public event Action Destroying;

        Action IEnablable.OnEnabling => Enabling;
        Action IEnablable.OnDisabling => Disabling;
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabled => null;

        public bool AllowInput { get; set; } = true;

        [SerializeField] private DarkscreenUI _darkscreen;
        [SerializeField] private SettingsUI _settingsUI;
        [SerializeField] private Animation _animation;

        private GameServices _gameServices;
        private EnableWithAnimation _enabler;

        [Space]
        [SerializeField] private string _gameSceneName;
        [SerializeField] private string _bossSceneName;


        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            if (_darkscreen)
                _darkscreen.SetToDisabled();
        }

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("gameServices Injected");
            _gameServices = gameServices;
        }

        private void Update()
        {
            if (_enabler.IsEnabled && Input.GetKeyDown(KeyCode.Escape))
            {
                using var _ = BlackboxHandle.Of(this).WriteScope("Esc");
                if (!AllowInput)
                {
                    BlackboxHandle.Of(this).Write("Input Blocked");
                    return;
                }

                Disable();
            }
        }

        public void Enable() => _enabler.Enable();
        public void Disable() => _enabler.Disable();
        public void SetToEnabled() => _enabler.SetToEnabled();
        public void SetToDisabled() => _enabler.SetToDisabled();

        #region Actions
        public void ToPlay()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Play");
            if (!AllowInput)
            {
                BlackboxHandle.Of(this).Write("Input Blocked");
                return;
            }

            if (_darkscreen)
            {
                _darkscreen.Enabled += () => SceneManager.LoadScene(_gameSceneName);
                _darkscreen.Enable();
            }
            else
                SceneManager.LoadScene(_gameSceneName);
        }

        public void ToGuide()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Guide");
            if (!AllowInput)
            {
                BlackboxHandle.Of(this).Write("Input Blocked");
                return;
            }

            Debug.LogWarning($"'[Menu] '{nameof(ToGuide)}'은(는) 아직 구현되지 않았습니다.", this);
        }

        public void ToBoss()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Boss");
            if (!AllowInput)
            {
                BlackboxHandle.Of(this).Write("Input Blocked");
                return;
            }

            if (_darkscreen)
            {
                _darkscreen.Enabled += () => SceneManager.LoadScene(_bossSceneName);
                _darkscreen.Enable();
            }
            else
                SceneManager.LoadScene(_bossSceneName);
        }

        public void ToSettings()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Settings");
            if (!AllowInput)
            {
                BlackboxHandle.Of(this).Write("Input Blocked");
                return;
            }

            if (_settingsUI)
            {
                BlackboxHandle.Of(this).Exert(_settingsUI, "Enable");
                _settingsUI.Enable();
            }
            else
            {
                Debug.LogWarning(
                    BlackboxHandle.Of(this).WriteMessage(
                        $"[Menu] {nameof(_settingsUI)}이(가) 유효하지 않기 때문에 ToSettings 메서드를 수행할 수 없습니다."),
                    this);
            }
        }

        public void ToQuit()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Quit");
            if (!AllowInput)
            {
                BlackboxHandle.Of(this).Write("Input Blocked");
                return;
            }

            if (_gameServices)
                _gameServices.Quit(this);
            else
            {
                Debug.LogWarning(
                    BlackboxHandle.Of(this).WriteMessage(
                        $"[Menu] {nameof(_gameServices)}이(가) 유효하지 않기 때문에 Quit 메서드를 수행할 수 없습니다."),
                    this);
            }
        }
        #endregion

        private void OnDestroy()
        {
            _enabler.Dispose();
            _enabler = null;

            Destroying?.Invoke();
        }
    }
}
