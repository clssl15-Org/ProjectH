using System;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using UI;
using BlackboxSystem;

namespace Game.Title
{
    public class Menu : MonoBehaviour, IEnablable, IInjectable<GameContext>
    {
        public event Action Enabling;
        public event Action Disabling;

        Action IEnablable.OnEnabling => Enabling;
        Action IEnablable.OnDisabling => Disabling;
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabled => null;

        [SerializeField] private GameContext _gameContext;
        [SerializeField] private DarkscreenUI _darkscreen;
        [SerializeField] private Animation _animation;
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

        void IInjectable<GameContext>.Inject(GameContext gameContext)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("GameContext Injected");
            _gameContext = gameContext;
        }

        private void Update()
        {
            if (_enabler.IsEnabled && Input.GetKeyDown(KeyCode.Escape))
                Disable();
        }

        public void Enable() => _enabler.Enable();
        public void Disable() => _enabler.Disable();
        public void SetToEnabled() => _enabler.SetToEnabled();
        public void SetToDisabled() => _enabler.SetToDisabled();

        #region Actions
        public void ToPlay()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Play");

            if (_darkscreen)
            {
                _darkscreen.Enabled += () => SceneManager.LoadScene(_gameSceneName);
                _darkscreen.Enable();
            }
            else
                SceneManager.LoadScene(_gameSceneName);
        }

        public void ToBoss()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Boss");

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
            Debug.LogWarning($"'[Menu] '{nameof(ToSettings)}'은(는) 아직 구현되지 않았습니다.", this);
        }

        public void ToQuit()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("To Quit");

            if (_gameContext)
                _gameContext.Quit(this);
            else
            {
                Debug.LogWarning(
                    BlackboxHandle.Of(this).Write(
                        $"[Menu] {nameof(_gameContext)}이(가) 유효하지 않기 때문에 Quit 메서드를 수행할 수 없습니다."),
                    this);
            }
        }
        #endregion

        private void OnDestroy()
        {
            _enabler.Dispose();
            _enabler = null;
        }
    }
}
