using System;
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
            add => _soundManager.BgmChanged += value;
            remove => _soundManager.BgmChanged -= value;
        }
        public override event Action<int> SfxChanged
        {
            add => _soundManager.SfxChanged += value;
            remove => _soundManager.SfxChanged -= value;
        }
        public override int BgmVolume => _soundManager.BgmVolume;
        public override int SfxVolume => _soundManager.SfxVolume;

        // Front
        [SerializeField] private GameAssetLibrary _gameAssetLibrary;

        // Properties
        private Management.SoundManager _soundManager;

        // Injections
        [SerializeField] private bool _injectOnSceneLoading = true;
        [SerializeField] private MonoBehaviour[] _injections;

        // Internal
        private static bool _isInitialized = false;


        // Content
        private void Awake()
        {
            if (_isInitialized)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    Ctx("인스턴스가 중복 생성되었습니다. 현재 생성 중인 인스턴스를 삭제합니다.")),
                    this);

                Destroy(gameObject);
                return;
            }

            _isInitialized = true;
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

            using var _ = BlackboxHandle.Of(this).WriteScope("인스턴스가 생성되었습니다.");

            _soundManager = GetComponentInChildren<Management.SoundManager>();
            if (_soundManager)
                BlackboxHandle.Of(this).Exert(_soundManager, "SoundManager 등록.");
            else 
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    "자식 컴포넌트에서 _soundManager을(를) 찾지 못했습니다."));

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _ = default)
        {
            var message = Ctx($"씬 '{scene.name}'이(가) 로드되었습니다.");
            using var __ = BlackboxHandle.Of(this).WriteScope(message);
            Debug.Log(message, this);

            if (TryFindScript<ScenarioManager>(scene, out var scenarioManager))
            {
                if (!TryFindScript<StageManager>(scene, out var stageManager))
                    throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                        Ctx("StageManager 컴포넌트를 찾는 데 실패했기 때문에 ScenarioManager를 초기화할 수 없습니다.")));

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
                        Ctx("Injector 컴포넌트를 찾는 데 실패했습니다. 주입을 수행할 수 없습니다.")));

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
                $"Bgm 볼륨을 {volume}(으)로 설정합니다.", context);

            _soundManager.SetBgmVolume(volume);
        }
        public override void SetSfxVolume(int volume, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"Sfx 볼륨을 {volume}(으)로 설정합니다.", context);

            _soundManager.SetSfxVolume(volume);
        }

        public override void SetPlayerName(string playerName, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"플레이어 이름을 '{playerName}'(으)로 설정합니다.", context);

            if (!_gameAssetLibrary)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(Ctx(
                    "GameAssetLibrary가 할당되지 않았습니다. 플레이어 이름을 설정할 수 없습니다.")));

            _gameAssetLibrary.TryGetCharacterInfo(World.Character.Player, out var playerInfo);
            playerInfo.Name = playerName;
        }

        public override void ChangeScene(string sceneName, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"씬을 '{sceneName}'(으)로 설정합니다.", context);

            try
            {
                SceneManager.LoadScene(sceneName);
                BlackboxHandle.Of(this).Write("씬 전환에 성공했습니다.");
            }
            catch (Exception ex)
            {
                Debug.LogError(BlackboxHandle.Of(this).WriteError(
                    $"씬 전환에 실패했습니다.\n{ex.ToString()}"));
                throw;
            }
        }

        public override void Quit(object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                "게임을 종료합니다.", context);

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
                            $"씬 '{scene.name}'의 '{found.name}'에서 {nameof(T)} 컴포넌트가 중복으로 발견되었습니다. " +
                            $"첫 번째로 발견된 객채 '{script.name}'의 컴포넌트를 사용합니다.")),
                            this);
                }
            }

            return script != null;
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("GameManager가 삭제되었습니다.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private string Ctx(string message) => $"[GameManager] {message}";
    }
}
