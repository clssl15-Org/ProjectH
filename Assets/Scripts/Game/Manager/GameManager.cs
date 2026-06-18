using System;
using System.Collections.Generic;
using BlackThunder.BlackboxSystem;
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
    /// 게임플레이 전체를 관리하는 매니저입니다.
    /// </summary>
    /// <remarks>
    /// 이 객체는 모든 씬에 걸쳐 존재하는 단일 인스턴스입니다.
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
        public override int BgmVolume => _soundManager.BgmVolume;
        public override int SfxVolume => _soundManager.SfxVolume;

        public override bool IsStage3Reached { get; set; }
        public override bool IsGameCleared { get; set; }
        public override bool PlayerHasDied => LevelManager.Instance.PlayerHasDied;

        // Front
        [SerializeField] private Configuration _configuration;
        [SerializeField] private GameAssetLibrary _gameAssetLibrary;
        [SerializeField] private string _firstSceneName = "Stage0 0";
        [SerializeField] private string _unlockSceneName = "Record_Unlocked";
        [SerializeField] private string _bossSceneName = "Stage3Boss";
        [Space]
        [SerializeField] private KeyCode _exportBlackboxKey = KeyCode.E;

        // Properties
        private Management.SoundManager _soundManager;
        private readonly HashSet<string> _reachedScenarioKeys = new();

        // Injections
        [SerializeField] private bool _injectOnSceneLoading = true;
        [SerializeField] private MonoBehaviour[] _injections;

        // Internal
        private static GameManager _instance;
        private static bool _isStarted;
        private BlackboxHandle _blackbox;


        // Content
        private void Awake()
        {
#if BLACKBOX
            BlackboxHandle.Configure(
                Application.persistentDataPath,
                Debug.Log,
                Debug.LogWarning,
                false,
                ExportFormat.Txt,
                FullExportOption.Full,
                OpenLogOption.Open,
                ExceptionHandlingOption.None,
                TargetTypes.Full);
#endif

            using var _ = BlackboxHandle.Of(this).Construct("게임 매니저 초기화를 시작합니다.", out _blackbox);

            if (_instance && _instance != this)
            {
                _blackbox.Write("중복 GameManager 인스턴스를 제거합니다.").With(_instance);
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);


            _soundManager = GetComponentInChildren<Management.SoundManager>();
            if (!_soundManager)
                throw new InvalidOperationException("자식 컴포넌트에서 _soundManager를(을) 찾을 수 없었습니다.");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            if (_isStarted) return;
            using var _ = _blackbox.Scope("게임 시작 상태를 초기화합니다.");

            _isStarted = true;

            // 게임 최초 시작 시
            if (_gameAssetLibrary.TryGetCharacterInfo(World.Character.Player, out var player))
                player.Name = _configuration.InitialPlayerName;
            else
                Debug.LogWarning(Ctx(
                    $"{nameof(_gameAssetLibrary)}에서 {World.Character.Player}를(을) 찾을 수 없기 때문에 " +
                    $"플레이어 이름을 '{_configuration.InitialPlayerName}'(으)로 변경할 수 없습니다."));

            IsStage3Reached = false;
            IsGameCleared = false;
            _reachedScenarioKeys.Clear();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode = default)
        {
            using var _ = _blackbox.Scope($"씬 로드 처리를 시작합니다. scene: {scene.name}");

            var message = Ctx($"씬 '{scene.name}'이(가) 로드되었습니다.");
            Debug.Log(message, this);

            if (TryFindScript<StageManager>(scene, out var stageManager))
            {
                _blackbox.Write("StageManager를 찾았습니다.").With(stageManager);
                stageManager.PlayerDied += () =>
                {
                    _blackbox.Write("플레이어 사망 흐름을 처리합니다.").With(stageManager);

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
                    throw new InvalidOperationException(Ctx("StageManager 컴포넌트를 찾는 데 실패했기 때문에 ScenarioManager를 초기화할 수 없습니다."));

                using (_blackbox.Exert(scenarioManager, "시나리오 매니저 초기화를 요청합니다."))
                scenarioManager.Initialize(stageManager);
            }

            Inject(scene);
            _soundManager.SetListenerIfPossible();
        }
        private void Inject(Scene scene)
        {
            using var _ = _blackbox.Scope($"씬 주입 처리를 시작합니다. scene: {scene.name}");

            if (_injectOnSceneLoading)
            {
                if (!TryFindScript<Injector>(scene, out var injector))
                    throw new InvalidOperationException(Ctx("Injector 컴포넌트를 찾는 데 실패했습니다. 주입을 수행할 수 없습니다."));

                using (_blackbox.Exert(injector, "게임 서비스와 추가 주입 객체 등록을 요청합니다."))
                {
                    injector.AddInjection(this, typeof(GameServices));
                    foreach (var injection in _injections)
                    {
                        if (injection)
                            injector.AddInjection(injection);
                    }

                    injector.Inject();
                }
            }
        }

        public override void SetBgmVolume(int volume, object context = null)
        {
            using var _ = context != null
                ? _blackbox.Scope($"BGM 볼륨 변경을 요청합니다. volume: {volume}").With(context)
                : _blackbox.Scope($"BGM 볼륨 변경을 요청합니다. volume: {volume}");

            using (_blackbox.Exert(_soundManager, "BGM 볼륨 변경을 전달합니다."))
            _soundManager.SetBgmVolume(volume);
        }
        public override void SetSfxVolume(int volume, object context = null)
        {
            using var _ = context != null
                ? _blackbox.Scope($"SFX 볼륨 변경을 요청합니다. volume: {volume}").With(context)
                : _blackbox.Scope($"SFX 볼륨 변경을 요청합니다. volume: {volume}");

            using (_blackbox.Exert(_soundManager, "SFX 볼륨 변경을 전달합니다."))
            _soundManager.SetSfxVolume(volume);
        }

        public override void SetPlayerName(string playerName, object context = null)
        {
            using var _ = context != null
                ? _blackbox.Scope($"플레이어 이름 변경을 요청합니다. playerName: {playerName}").With(context)
                : _blackbox.Scope($"플레이어 이름 변경을 요청합니다. playerName: {playerName}");

            if (!_gameAssetLibrary)
                throw new InvalidOperationException(Ctx(
                    "GameAssetLibrary가 할당되지 않았습니다. 플레이어 이름을 설정할 수 없습니다."));

            _gameAssetLibrary.TryGetCharacterInfo(World.Character.Player, out var playerInfo);
            playerInfo.Name = playerName;
        }

        public override bool ConsumeFirstScenarioArrival(string scenarioKey, object context = null)
        {
            using var _ = context != null
                ? _blackbox.Scope($"시나리오 최초 도달 여부를 확인합니다. key: {scenarioKey}").With(context)
                : _blackbox.Scope($"시나리오 최초 도달 여부를 확인합니다. key: {scenarioKey}");

            if (string.IsNullOrWhiteSpace(scenarioKey))
            {
                _blackbox.Write("비어 있는 시나리오 키가 입력되어 false로 처리합니다.");
                Debug.LogWarning(Ctx(
                    "시나리오 최초도달 키가 비어 있습니다. 안전하게 재도달로 처리합니다."),
                    this);
                return false;
            }

            bool isFirstArrival = _reachedScenarioKeys.Add(scenarioKey);
            _blackbox.Write($"시나리오 최초 도달 결과입니다. key: {scenarioKey}, isFirstArrival: {isFirstArrival}");

            return isFirstArrival;
        }

        public override void ToFirstScene(object context = null)
        {
            using var _ = context != null
                ? _blackbox.Scope("첫 씬으로 이동을 요청합니다.").With(context)
                : _blackbox.Scope("첫 씬으로 이동을 요청합니다.");

            ChangeScene(_firstSceneName, context);
        }

        public override void ChangeScene(string sceneName, object context = null, bool preservePlayerProgress = true)
        {
            using var _ = context != null
                ? _blackbox.Scope($"씬 전환을 요청합니다. scene: {sceneName}, preservePlayerProgress: {preservePlayerProgress}").With(context)
                : _blackbox.Scope($"씬 전환을 요청합니다. scene: {sceneName}, preservePlayerProgress: {preservePlayerProgress}");

            try
            {
                PreparePlayerProgressForSceneChange(preservePlayerProgress);
                SceneManager.LoadScene(sceneName);
            }
            catch (Exception ex)
            {
                _blackbox.WriteError($"씬 전환에 실패했습니다. scene: {sceneName}, error: {ex}");
                Debug.LogError($"씬 전환에 실패했습니다.\n{ex.ToString()}");
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

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ResetState();
                return;
            }

            SkillManager.ClearPersistedSkillLoadout();
            Actors.PlayerSystem.Player.ClearPersistedProgress();
        }

        private void Update()
        {
#if DEBUG_MODE || PLAYER_DEBUG_MODE
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(_exportBlackboxKey))
                _blackbox.Export();
#endif
        }

        public override void Quit(object context = null)
        {
            using var _ = context != null
                ? _blackbox.Scope("게임 종료를 요청합니다.").With(context)
                : _blackbox.Scope("게임 종료를 요청합니다.");

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
                        Debug.LogWarning(Ctx(
                            $"씬 '{scene.name}'의 '{found.name}'에서 {nameof(T)} 컴포넌트가 중복으로 발견되었습니다. " +
                            $"첫 번째로 발견된 객체 '{script.name}'의 컴포넌트를 사용합니다."),
                            this);
                }
            }

            return script != null;
        }

        private void OnDestroy()
        {
            using var _ = _blackbox.Scope("게임 매니저를 정리합니다.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private string Ctx(string message) => $"[GameManager] {message}";
    }
}
