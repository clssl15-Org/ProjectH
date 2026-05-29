using System;
using System.Collections.Generic;
using BlackboxSystem;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Stage;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    /// <summary>
    /// 게임?��?��?�� ?��체�?? �?리하?�� 매니????��?��?��.
    /// </summary>
    /// <remarks>
    /// ?�� 객체?�� 모든 ?��?�� 걸쳐 존재?��?�� ?��?�� ?��?��?��?��?��?��?��.
    /// </remarks>
    public sealed class GameManager : GameServices
    {
        // Forwardings
        public override event Action<int> BgmChanged
        {
            add
            {
                if (_soundManager != null)
                    _soundManager.BgmChanged += value;
            }
            remove
            {
                if (_soundManager != null)
                    _soundManager.BgmChanged -= value;
            }
        }
        public override event Action<int> SfxChanged
        {
            add
            {
                if (_soundManager != null)
                    _soundManager.SfxChanged += value;
            }
            remove
            {
                if (_soundManager != null)
                    _soundManager.SfxChanged -= value;
            }
        }
        public override int BgmVolume => _soundManager != null ? _soundManager.BgmVolume : 70;
        public override int SfxVolume => _soundManager != null ? _soundManager.SfxVolume : 70;

        public override bool IsStage3Reached { get; set; }
        public override bool IsGameCleared { get; set; }
        public override bool PlayerHasDied => LevelManager.Instance.PlayerHasDied;

        // Front
        [SerializeField] private Configuration _configuration;
        [SerializeField] private GameAssetLibrary _gameAssetLibrary;
        [SerializeField] private string _firstSceneName = "Stage0 0";
        [SerializeField] private string _unlockSceneName = "Record_Unlocked";
        [SerializeField] private string _bossSceneName = "Stage3Boss";

        // Properties
        private Management.SoundManager _soundManager;
        private readonly HashSet<string> _reachedScenarioKeys = new();

        // Injections
        [SerializeField] private bool _injectOnSceneLoading = true;
        [SerializeField] private MonoBehaviour[] _injections;

        // Internal
        private static GameManager _instance;
        private static bool _isStarted;


        // Content
        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            BlackboxHandle.Configure(
                logDirectory: Application.persistentDataPath,
                normalLogger: Debug.Log,
                warningLogger: Debug.LogWarning,
                strongReference: false,
                exportFormat: ExportFormat.Html,
                fullExportOption: FullExportOption.Full,
                openLogOption: OpenLogOption.Open,
                exceptionHandlingOption: ExceptionHandlingOption.CrashExport);

            using var _ = BlackboxHandle.Of(this).WriteScope("?��?��?��?���? ?��?��?��?��?��?��?��.");

            _soundManager = GetComponentInChildren<Management.SoundManager>();
            if (_soundManager)
                BlackboxHandle.Of(this).Exert(_soundManager, "SoundManager ?���?.");
            else 
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    "?��?�� 컴포?��?��?��?�� _soundManager?��(�?) 찾�?? 못했?��?��?��."));

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            if (_isStarted) return;
            _isStarted = true;

            // 게임 최초 ?��?�� ?��
            if (_gameAssetLibrary.TryGetCharacterInfo(World.Character.Player, out var player))
                player.Name = _configuration.InitialPlayerName;
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteError(Ctx(
                    $"{nameof(_gameAssetLibrary)}?��?�� {World.Character.Player}?��(�?) 찾�?? 못했�? ?��문에 " +
                    $"?��?��?��?�� ?��름을 '{_configuration.InitialPlayerName}'(?��)�? �?경할 ?�� ?��?��?��?��.")));

            IsStage3Reached = false;
            IsGameCleared = false;
            _reachedScenarioKeys.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _ = default)
        {
            var message = Ctx($"?�� '{scene.name}'?��(�?) 로드?��?��?��?��?��.");
            using var __ = BlackboxHandle.Of(this).WriteScope(message);
            Debug.Log(message, this);

            if (TryFindScript<StageManager>(scene, out var stageManager))
            {
                stageManager.PlayerDied += () =>
                {
                    using var _ = BlackboxHandle.Of(this).WriteScope("?��?��?��?�� ?���?");

                    LevelManager.Instance.ResetState();
                    LevelManager.Instance.MarkPlayerDied();
                    
                    if (scene.name == _bossSceneName)
                        ChangeScene(_unlockSceneName, preservePlayerProgress: false);
                    else
                        ChangeScene(_firstSceneName, preservePlayerProgress: false);
                };
            }

            if (TryFindScript<ScenarioManager>(scene, out var scenarioManager))
            {
                if (!stageManager)
                    throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                        Ctx("StageManager 컴포?��?���? 찾는 ?�� ?��?��?���? ?��문에 ScenarioManager�? 초기?��?�� ?�� ?��?��?��?��.")));

                BlackboxHandle.Of(this).Exert(scenarioManager, "Initialize");
                scenarioManager.Initialize(stageManager);
            }

            Inject(scene);
            _soundManager.SetListenerIfPossible();
        }
        private void Inject(Scene scene)
        {
            if (_injectOnSceneLoading)
            {
                if (!TryFindScript<Injector>(scene, out var injector))
                    throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                        Ctx("Injector 컴포?��?���? 찾는 ?�� ?��?��?��?��?��?��. 주입?�� ?��?��?�� ?�� ?��?��?��?��.")));

                injector.AddInjection(this, typeof(GameServices));
                foreach (var injection in _injections)
                {
                    if (injection)
                        injector.AddInjection(injection);
                }
                injector.Inject();
            }
        }

        public override void SetBgmVolume(int volume, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"Bgm 볼륨?�� {volume}(?��)�? ?��?��?��?��?��.", context);

            _soundManager.SetBgmVolume(volume);
        }
        public override void SetSfxVolume(int volume, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"Sfx 볼륨?�� {volume}(?��)�? ?��?��?��?��?��.", context);

            _soundManager.SetSfxVolume(volume);
        }

        public override void SetPlayerName(string playerName, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"?��?��?��?�� ?��름을 '{playerName}'(?��)�? ?��?��?��?��?��.", context);

            if (!_gameAssetLibrary)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(Ctx(
                    "GameAssetLibrary�? ?��?��?���? ?��?��?��?��?��. ?��?��?��?�� ?��름을 ?��?��?�� ?�� ?��?��?��?��.")));

            _gameAssetLibrary.TryGetCharacterInfo(World.Character.Player, out var playerInfo);
            playerInfo.Name = playerName;
        }

        public override bool ConsumeFirstScenarioArrival(string scenarioKey, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"?��?��리오 최초?��?�� ?���?�? ?��?��?��?��?��. Key: '{scenarioKey}'", context);

            if (string.IsNullOrWhiteSpace(scenarioKey))
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                    "?��?��리오 최초?��?�� ?���? 비어 ?��?��?��?��. ?��?��?���? ?��?��?���? 처리?��?��?��.")),
                    this);
                return false;
            }

            bool isFirstArrival = _reachedScenarioKeys.Add(scenarioKey);
            BlackboxHandle.Of(this).Write(isFirstArrival
                ? "?���? ?��?��?���? ?��??? ?��?��리오?��?��?��. 최초?��?���? 기록?��?��?��."
                : "?���? ?��?��?�� ?��?��리오?��?��?��. ?��?��?���? 처리?��?��?��.");

            return isFirstArrival;
        }

        public override void ToFirstScene(object context = null) => ChangeScene(_firstSceneName, context);
        public override void ChangeScene(string sceneName, object context = null, bool preservePlayerProgress = true)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"?��?�� '{sceneName}'(?��)�? ?��?��?��?��?��.", context);

            try
            {
                PreparePlayerProgressForSceneChange(preservePlayerProgress);
                SceneManager.LoadScene(sceneName);
                BlackboxHandle.Of(this).Write("?�� ?��?��?�� ?��공했?��?��?��.");
            }
            catch (Exception ex)
            {
                Debug.LogError(BlackboxHandle.Of(this).WriteError(
                    $"?�� ?��?��?�� ?��?��?��?��?��?��.\n{ex.ToString()}"));
                throw;
            }
        }

        private static void PreparePlayerProgressForSceneChange(bool preservePlayerProgress)
        {
            if (preservePlayerProgress)
            {
                Actors.PlayerSystem.Player.TryPersistCurrentPlayerProgress();
                return;
            }

            if (global::LevelManager.Instance != null)
            {
                global::LevelManager.Instance.ResetState();
                return;
            }

            global::SkillManager.ClearPersistedSkillLoadout();
            Actors.PlayerSystem.Player.ClearPersistedProgress();
        }

        public override void Quit(object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                "게임?�� 종료?��?��?��.", context);

#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }


        private bool TryFindScript<T>(Scene scene, out T script) where T : MonoBehaviour
        {
            script = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var founds = root.GetComponentsInChildren<T>(true);
                foreach (var found in founds)
                {
                    if (script == null)
                        script = found;
                    else
                        Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                            $"?�� '{scene.name}'?�� '{found.name}'?��?�� {nameof(T)} 컴포?��?���? 중복?���? 발견?��?��?��?��?��. " +
                            $"�? 번째�? 발견?�� 객채 '{script.name}'?�� 컴포?��?���? ?��?��?��?��?��.")),
                            this);
                }
            }

            return script != null;
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("GameManager�? ?��?��?��?��?��?��?��.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private string Ctx(string message) => $"[GameManager] {message}";
    }
}
