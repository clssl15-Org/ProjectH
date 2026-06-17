using System;
using Infrastructure;
using Sound;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Title
{
    public class Menu : MonoBehaviour,
        IInjectable<GameServices>,
        IInjectable<BgmPlayManager>,
        IEnablable,
        IInputLayerSubject
    {
        public event Action Enabling;
        public event Action Disabling;
        public event Action Destroying;

        Action IEnablable.OnEnabling => Enabling;
        Action IEnablable.OnDisabling => Disabling;
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabled => null;

        public bool AllowInput { get; set; } = true;
        bool IInputLayerSubject.IsTrigger { get; } = false;

        public event Action OpenGuide;

        [SerializeField] private DarkscreenUI _darkscreen;
        [SerializeField] private SettingsUI _settingsUI;
        [SerializeField] private Animation _animation;

        private GameServices _gameServices;
        private BgmPlayManager _bgmPlayManager;
        private EnableWithAnimation _enabler;

        [Space]
        [SerializeField] private string _gameSceneName;


        private void Awake()
        {

            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);

            if (_darkscreen)
            {
                _darkscreen.SetToDisabled();
            }
        }

        void IInjectable<GameServices>.Inject(GameServices gameServices)
        {
            _gameServices = gameServices;
        }
        void IInjectable<BgmPlayManager>.Inject(BgmPlayManager bgmPlayManager)
        {
            _bgmPlayManager = bgmPlayManager;
        }

        private void Update()
        {
            if (_enabler.IsEnabled && Input.GetKeyDown(KeyCode.Escape))
            {
                if (!AllowInput)
                {
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
            if (!AllowInput)
            {
                return;
            }

            if (_darkscreen)
            {

                BgmPlayManager.Stop();
                _darkscreen.CloseScreen(LoadGameScene);
            }
            else
                LoadGameScene();
        }

        private void LoadGameScene()
        {
            if (_gameServices)
            {
                _gameServices.ChangeScene(_gameSceneName, this, preservePlayerProgress: false);
                return;
            }

            if (global::LevelManager.Instance != null)
                global::LevelManager.Instance.ResetState();
            else
            {
                global::SkillManager.ClearPersistedSkillLoadout();
                Actors.PlayerSystem.Player.ClearPersistedProgress();
            }

            SceneManager.LoadScene(_gameSceneName);
        }

        public void ToGuide()
        {
            if (!AllowInput)
            {
                return;
            }

            OpenGuide?.Invoke();
        }

        public void ToSettings()
        {
            if (!AllowInput)
            {
                return;
            }

            if (_settingsUI)
            {
                _settingsUI.Enable();
            }
            else
            {
                Debug.LogWarning($"[Menu] {nameof(_settingsUI)}��(��) ��ȿ���� �ʱ� ������ ToSettings �޼��带 ������ �� �����ϴ�.",
                    this);
            }
        }

        public void ToQuit()
        {
            if (!AllowInput)
            {
                return;
            }

            if (_gameServices)
                _gameServices.Quit(this);
            else
            {
                Debug.LogWarning(
                    $"[Menu] {nameof(_gameServices)}��(��) ��ȿ���� �ʱ� ������ Quit �޼��带 ������ �� �����ϴ�.",
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
