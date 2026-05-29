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
    /// Í≤åÏûÑ?îå?†à?ù¥ ?†ÑÏ≤¥Î?? Í¥?Î¶¨Ìïò?äî Îß§Îãà????ûÖ?ãà?ã§.
    /// </summary>
    /// <remarks>
    /// ?ù¥ Í∞ùÏ≤¥?äî Î™®Îì† ?î¨?óê Í±∏Ï≥ê Ï°¥Ïû¨?ïò?äî ?ã®?ùº ?ù∏?ä§?Ñ¥?ä§?ûÖ?ãà?ã§.
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

            using var _ = BlackboxHandle.Of(this).WriteScope("?ù∏?ä§?Ñ¥?ä§Í∞? ?Éù?Ñ±?êò?óà?äµ?ãà?ã§.");

            _soundManager = GetComponentInChildren<Management.SoundManager>();
            if (_soundManager)
                BlackboxHandle.Of(this).Exert(_soundManager, "SoundManager ?ì±Î°?.");
            else 
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    "?ûê?ãù Ïª¥Ìè¨?Ñå?ä∏?óê?Ñú _soundManager?ùÑ(Î•?) Ï∞æÏ?? Î™ªÌñà?äµ?ãà?ã§."));

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            if (_isStarted) return;
            _isStarted = true;

            // Í≤åÏûÑ ÏµúÏ¥à ?ãú?ûë ?ãú
            if (_gameAssetLibrary.TryGetCharacterInfo(World.Character.Player, out var player))
                player.Name = _configuration.InitialPlayerName;
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteError(Ctx(
                    $"{nameof(_gameAssetLibrary)}?óê?Ñú {World.Character.Player}?ùÑ(Î•?) Ï∞æÏ?? Î™ªÌñàÍ∏? ?ïåÎ¨∏Ïóê " +
                    $"?îå?†à?ù¥?ñ¥ ?ù¥Î¶ÑÏùÑ '{_configuration.InitialPlayerName}'(?úº)Î°? Î≥?Í≤ΩÌï† ?àò ?óÜ?äµ?ãà?ã§.")));

            IsStage3Reached = false;
            IsGameCleared = false;
            _reachedScenarioKeys.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _ = default)
        {
            var message = Ctx($"?î¨ '{scene.name}'?ù¥(Í∞?) Î°úÎìú?êò?óà?äµ?ãà?ã§.");
            using var __ = BlackboxHandle.Of(this).WriteScope(message);
            Debug.Log(message, this);

            if (TryFindScript<StageManager>(scene, out var stageManager))
            {
                stageManager.PlayerDied += () =>
                {
                    using var _ = BlackboxHandle.Of(this).WriteScope("?îå?†à?ù¥?ñ¥ ?Ç¨Îß?");

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
                        Ctx("StageManager Ïª¥Ìè¨?Ñå?ä∏Î•? Ï∞æÎäî ?ç∞ ?ã§?å®?ñàÍ∏? ?ïåÎ¨∏Ïóê ScenarioManagerÎ•? Ï¥àÍ∏∞?ôî?ï† ?àò ?óÜ?äµ?ãà?ã§.")));

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
                        Ctx("Injector Ïª¥Ìè¨?Ñå?ä∏Î•? Ï∞æÎäî ?ç∞ ?ã§?å®?ñà?äµ?ãà?ã§. Ï£ºÏûÖ?ùÑ ?àò?ñâ?ï† ?àò ?óÜ?äµ?ãà?ã§.")));

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
                $"Bgm Î≥ºÎ•®?ùÑ {volume}(?úº)Î°? ?Ñ§?†ï?ï©?ãà?ã§.", context);

            _soundManager.SetBgmVolume(volume);
        }
        public override void SetSfxVolume(int volume, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"Sfx Î≥ºÎ•®?ùÑ {volume}(?úº)Î°? ?Ñ§?†ï?ï©?ãà?ã§.", context);

            _soundManager.SetSfxVolume(volume);
        }

        public override void SetPlayerName(string playerName, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"?îå?†à?ù¥?ñ¥ ?ù¥Î¶ÑÏùÑ '{playerName}'(?úº)Î°? ?Ñ§?†ï?ï©?ãà?ã§.", context);

            if (!_gameAssetLibrary)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(Ctx(
                    "GameAssetLibraryÍ∞? ?ï†?ãπ?êòÏß? ?ïä?ïò?äµ?ãà?ã§. ?îå?†à?ù¥?ñ¥ ?ù¥Î¶ÑÏùÑ ?Ñ§?†ï?ï† ?àò ?óÜ?äµ?ãà?ã§.")));

            _gameAssetLibrary.TryGetCharacterInfo(World.Character.Player, out var playerInfo);
            playerInfo.Name = playerName;
        }

        public override bool ConsumeFirstScenarioArrival(string scenarioKey, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"?ãú?ÇòÎ¶¨Ïò§ ÏµúÏ¥à?èÑ?ã¨ ?ó¨Î∂?Î•? ?ôï?ù∏?ï©?ãà?ã§. Key: '{scenarioKey}'", context);

            if (string.IsNullOrWhiteSpace(scenarioKey))
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(Ctx(
                    "?ãú?ÇòÎ¶¨Ïò§ ÏµúÏ¥à?èÑ?ã¨ ?Ç§Í∞? ÎπÑÏñ¥ ?ûà?äµ?ãà?ã§. ?ïà?†Ñ?ïòÍ≤? ?û¨?èÑ?ã¨Î°? Ï≤òÎ¶¨?ï©?ãà?ã§.")),
                    this);
                return false;
            }

            bool isFirstArrival = _reachedScenarioKeys.Add(scenarioKey);
            BlackboxHandle.Of(this).Write(isFirstArrival
                ? "?ïÑÏß? ?èÑ?ã¨?ïòÏß? ?ïä??? ?ãú?ÇòÎ¶¨Ïò§?ûÖ?ãà?ã§. ÏµúÏ¥à?èÑ?ã¨Î°? Í∏∞Î°ù?ï©?ãà?ã§."
                : "?ù¥ÎØ? ?èÑ?ã¨?ïú ?ãú?ÇòÎ¶¨Ïò§?ûÖ?ãà?ã§. ?û¨?èÑ?ã¨Î°? Ï≤òÎ¶¨?ï©?ãà?ã§.");

            return isFirstArrival;
        }

        public override void ToFirstScene(object context = null) => ChangeScene(_firstSceneName, context);
        public override void ChangeScene(string sceneName, object context = null, bool preservePlayerProgress = true)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"?î¨?ùÑ '{sceneName}'(?úº)Î°? ?Ñ§?†ï?ï©?ãà?ã§.", context);

            try
            {
                PreparePlayerProgressForSceneChange(preservePlayerProgress);
                SceneManager.LoadScene(sceneName);
                BlackboxHandle.Of(this).Write("?î¨ ?†Ñ?ôò?óê ?Ñ±Í≥µÌñà?äµ?ãà?ã§.");
            }
            catch (Exception ex)
            {
                Debug.LogError(BlackboxHandle.Of(this).WriteError(
                    $"?î¨ ?†Ñ?ôò?óê ?ã§?å®?ñà?äµ?ãà?ã§.\n{ex.ToString()}"));
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
                "Í≤åÏûÑ?ùÑ Ï¢ÖÎ£å?ï©?ãà?ã§.", context);

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
                            $"?î¨ '{scene.name}'?ùò '{found.name}'?óê?Ñú {nameof(T)} Ïª¥Ìè¨?Ñå?ä∏Í∞? Ï§ëÎ≥µ?úºÎ°? Î∞úÍ≤¨?êò?óà?äµ?ãà?ã§. " +
                            $"Ï≤? Î≤àÏß∏Î°? Î∞úÍ≤¨?êú Í∞ùÏ±Ñ '{script.name}'?ùò Ïª¥Ìè¨?Ñå?ä∏Î•? ?Ç¨?ö©?ï©?ãà?ã§.")),
                            this);
                }
            }

            return script != null;
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("GameManagerÍ∞? ?Ç≠?†ú?êò?óà?äµ?ãà?ã§.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private string Ctx(string message) => $"[GameManager] {message}";
    }
}
