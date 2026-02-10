using System;
using BlackboxSystem;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public sealed class GameManager : GameContext
    {
        // Forwardings
        public override event Action<float> BgmChanged
        {
            add => _soundManager.BgmChanged += value;
            remove => _soundManager.BgmChanged -= value;
        }
        public override event Action<float> SfxChanged
        {
            add => _soundManager.SfxChanged += value;
            remove => _soundManager.SfxChanged -= value;
        }
        public override int BgmVolume => _soundManager.BgmVolume;
        public override int SfxVolume => _soundManager.SfxVolume;

        // Properties
        private Management.SoundManager _soundManager;

        // Internal
        [SerializeField] private bool _injectSelfOnSceneLoading = true;
        private static bool _isInitialized = false;


        // Content
        private void Awake()
        {
            if (_isInitialized)
            {
                Debug.LogWarning(BlackboxHandle.Of(this).Write(
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

            SceneManager.sceneLoaded += OnSceneLoaded;

            _soundManager = new Management.SoundManager();
            BlackboxHandle.Of(this).Exert(_soundManager, "SoundManager를 생성했습니다.");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            using var __ = BlackboxHandle.Of(this).WriteScope($"씬 '{scene.name}'이(가) 로드되었습니다.");
            InjectSelf(scene);
        }
        private void InjectSelf(Scene scene)
        {
            if (_injectSelfOnSceneLoading)
            {
                Injector injector = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    var foundInjectors = root.GetComponentsInChildren<Injector>(true);
                    foreach (var foundInjector in foundInjectors)
                    {
                        if (injector == null)
                            injector = foundInjector;
                        else
                            Debug.LogWarning(BlackboxHandle.Of(this).Write(Ctx(
                                $"씬 '{scene.name}'의 '{foundInjector.name}'에서 Injector 컴포넌트가 중복으로 발견되었습니다. " +
                                $"첫 번째로 발견된 객채 '{injector.name}'의 컴포넌트를 사용합니다.")),
                                this);
                    }
                }

                if (!injector)
                    throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                        Ctx("Injector 컴포넌트를 찾는 데 실패했습니다. injector에 자기 주입을 수행할 수 없습니다.")));

                injector.AddInjection(this, typeof(GameContext));
                injector.Inject();
            }
        }

        public override void SetBgmVolume(int volume, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"Bgm 볼륨을 {volume}으로 설정합니다.", context);

            _soundManager.SetBgmVolume(volume);
        }
        public override void SetSfxVolume(int volume, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"Sfx 볼륨을 {volume}으로 설정합니다.", context);

            _soundManager.SetSfxVolume(volume);
        }

        public override void ChangeScene(string sceneName, object context = null)
        {
            using var _ = BlackboxHandle.Of(this).WriteOrExertedScope(
                $"씬을 {sceneName}(으)로 설정합니다.", context);

            try
            {
                SceneManager.LoadScene(sceneName);
                BlackboxHandle.Of(this).Write("씬 전환에 성공했습니다.");
            }
            catch (Exception ex)
            {
                Debug.LogError(BlackboxHandle.Of(this).CrashExport(
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

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("GameManager가 삭제되었습니다.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private string Ctx(string message) => $"[GameManager] {message}";
    }
}
