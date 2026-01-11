using System;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Title
{
    public class Menu : MonoBehaviour, IEnablable
    {
        public event Action Enabling;
        public event Action Disabling;

        Action IEnablable.OnEnabling => Enabling;
        Action IEnablable.OnDisabling => Disabling;
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabled => null;

        [SerializeField] private Darkscreen _darkscreen;
        [SerializeField] private Animation _animation;
        private EnableWithAnimation _enabler;

        [Space]
        [SerializeField] private string _gameSceneName;
        [SerializeField] private string _bossSceneName;


        private void Awake()
        {
            _enabler = new EnableWithAnimation(_animation)
                .InitializeWithIEnablable(this);

            if (_darkscreen)
                _darkscreen.SetToDisabled();
        }

        private void Update()
        {
            if (_enabler.Enabled && Input.GetKeyDown(KeyCode.Escape))
                Disable();
        }

        public void Enable() => _enabler.Enable();
        public void Disable() => _enabler.Disable();
        public void SetToEnabled() => _enabler.SetToEnabled();
        public void SetToDisabled() => _enabler.SetToDisabled();

        #region Actions
        public void ToPlay()
        {
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
            if (_darkscreen)
            {
                _darkscreen.Enabled += () => SceneManager.LoadScene(_bossSceneName);
                _darkscreen.Enable();
            }
            else
                SceneManager.LoadScene(_bossSceneName);
        }

        public void ToSetting()
        {
            Debug.LogWarning($"'{nameof(ToSetting)}'은(는) 아직 구현되지 않았습니다.", this);
        }

        public void ToQuit()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
        #endregion

        private void OnDestroy()
        {
            _enabler.Dispose();
            _enabler = null;
        }
    }
}
