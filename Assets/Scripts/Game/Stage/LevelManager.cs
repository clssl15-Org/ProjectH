using System.Collections;
using System.Collections.Generic;
using Actors;
using Actors.PlayerSystem;
using BlackThunder.BlackboxSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelType
{
    None,
    Normal,
    BossMap,
    NextStage,
}
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    private static bool _isInitialized = false;
    public SpawnManager SpawnManager => _spawnManager;
    public SoundManager SoundManager => _soundManager;
    private SpawnManager _spawnManager;
    private SpawnManager _spawnManagerOnLevelRoot;
    private SoundManager _soundManager;
    private BlackboxHandle _blackbox;

    public bool PlayerHasDied { get; private set; }
    public bool IsPlayerDeathRestartPending { get; private set; }
    public int CurrentStage { get; private set; } = 0;

    private List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    private int exploreIndex = 0;
    private int maxExploreCount = 5;
    private int exploreCount = 0;
    public int ExploreCount => exploreCount;

    private void Awake()
    {
        using var _ = BlackboxHandle.Of(this).Construct("레벨 매니저 초기화를 시작합니다.", out _blackbox);

        if (_isInitialized)
        {
            _blackbox.Write("중복 LevelManager 인스턴스를 제거합니다.").With(Instance);
            Destroy(gameObject);
            return;
        }

        _isInitialized = true;
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _spawnManagerOnLevelRoot = GetComponent<SpawnManager>();
        _soundManager = GetComponent<SoundManager>();

        SceneManager.sceneLoaded += OnSceneLoaded;
        RebindSpawnManagerForScene(SceneManager.GetActiveScene());

        ResetState();
        SyncCurrentStageFromScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        using var _ = _blackbox.Scope("레벨 매니저를 정리합니다.");

        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        using var _ = _blackbox.Scope($"씬 로드에 맞춰 레벨 상태를 갱신합니다. scene: {scene.name}");

        RebindSpawnManagerForScene(scene);
        SyncCurrentStageFromScene(scene);
    }

    /// <summary>
    /// DDOL SpawnManager를 유지한 채, 씬마다 둔 SpawnManager(WaveDataList 등) 설정만 현재 씬 기준으로 반영합니다.
    /// sceneLoaded는 해당 씬의 Start 호출 전에 불리므로 MonsterSpawner 등록 시점과 맞습니다.
    /// </summary>
    private void RebindSpawnManagerForScene(Scene scene)
    {
        using var _ = _blackbox.Scope($"씬 SpawnManager 설정을 다시 연결합니다. scene: {scene.name}");

        if (_spawnManagerOnLevelRoot == null)
        {
            SpawnManager inScene = FindSceneSpawnManagerConfigurationSource(scene);
            SpawnManager next = inScene != null ? inScene : null;

            if (next == null)
            {
                Debug.LogWarning("SpawnManager를 찾을 수 없습니다. 씬 또는 LevelManager 오브젝트에 배치해 주세요.", this);
                return;
            }

            if (_spawnManager != null && _spawnManager != next)
                _spawnManager.Clear();

            _spawnManager = next;
            _spawnManager.RefreshClearObjectForLoadedScene(scene);
            return;
        }

        SpawnManager sceneSource = FindSceneSpawnManagerConfigurationSource(scene);

        if (_spawnManager != null && _spawnManager != _spawnManagerOnLevelRoot)
            _spawnManager.Clear();

        _spawnManagerOnLevelRoot.Clear();

        if (sceneSource != null && sceneSource != _spawnManagerOnLevelRoot)
            _spawnManagerOnLevelRoot.ApplyConfigurationFrom(sceneSource);

        _spawnManager = _spawnManagerOnLevelRoot;
        _spawnManager.RefreshClearObjectForLoadedScene(scene);
    }

    /// <summary>
    /// 씬에서 waveDataList·clearObject 등을 읽어올 SpawnManager를 고릅니다.
    /// 곧 파괴되는 LevelManager 복제본보다 standalone SpawnManager를 우선합니다.
    /// </summary>
    private static SpawnManager FindSceneSpawnManagerConfigurationSource(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return null;

        SpawnManager standaloneCandidate = null;
        SpawnManager levelManagerCandidate = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (SpawnManager spawnManager in root.GetComponentsInChildren<SpawnManager>(true))
            {
                var levelManager = spawnManager.GetComponent<LevelManager>();
                if (levelManager != null)
                {
                    if (LevelManager.Instance != null && levelManager == LevelManager.Instance)
                        continue;

                    levelManagerCandidate ??= spawnManager;
                    continue;
                }

                standaloneCandidate ??= spawnManager;
            }
        }

        return standaloneCandidate ?? levelManagerCandidate;
    }

    public void ResetState()
    {
        using var _ = _blackbox.Scope("레벨 진행 상태를 초기화합니다.");

        if (RelicManager.Instance != null)
            RelicManager.Instance.ClearAllOwnedRelics();

        SkillManager.ClearPersistedSkillLoadout();
        Player.ClearPersistedProgress();

        CurrentStage = 0;

        exploreIndex = 0;
        maxExploreCount = 5;
        exploreCount = 0;

        ShuffleAndPick();
    }
    public void MarkPlayerDied()
    {
        using var _ = _blackbox.Scope("플레이어 사망 상태를 표시합니다.");

        PlayerHasDied = true;
        IsPlayerDeathRestartPending = true;
    }
    public bool ConsumePlayerDeathRestart()
    {
        using var _ = _blackbox.Scope("플레이어 사망 재시작 요청을 소비합니다.");

        if (!IsPlayerDeathRestartPending)
            return false;

        IsPlayerDeathRestartPending = false;
        return true;
    }
    public void MoveNextLevel(LevelType nextLevelType)
    {
        using var _ = _blackbox.Scope($"다음 레벨 이동을 계산합니다. nextLevelType: {nextLevelType}");

        string nextScene = default;
        switch (nextLevelType)
        {
            case LevelType.Normal:
                exploreCount += 1;
                if (exploreCount <= maxExploreCount)
                {
                    exploreIndex = (exploreIndex + 1) % numbers.Count;
                    nextScene = $"Stage{CurrentStage} {numbers[exploreIndex]}";
                }
                else
                {
                    nextScene = $"Stage{CurrentStage}Boss";
                }
                break;
            case LevelType.BossMap:
                nextScene = $"Stage{CurrentStage}Boss";
                break;
            case LevelType.NextStage:
                CurrentStage += 1;
                exploreCount = 0;
                nextScene = $"Stage{CurrentStage}_LargeMap";
                break;
            default:
                Debug.LogError("Invalid level type");
                return;
        }

        LoadNextScene(nextScene);
    }
    public void LoadNextScene(string sceneName)
    {
        using var _ = _blackbox.Scope($"다음 씬을 로드합니다. scene: {sceneName}");

        Player.PersistCurrentPlayerProgress();
        SceneManager.LoadScene(sceneName);
    }

    private void SyncCurrentStageFromScene(Scene scene)
    {
        if (TryGetStageNumber(scene.name, out int stageNumber))
            CurrentStage = stageNumber;
    }

    private static bool TryGetStageNumber(string sceneName, out int stageNumber)
    {
        stageNumber = 0;

        int stageTextIndex = sceneName.IndexOf("Stage", System.StringComparison.Ordinal);
        if (stageTextIndex < 0)
            return false;

        int digitStartIndex = stageTextIndex + "Stage".Length;
        if (digitStartIndex >= sceneName.Length || !char.IsDigit(sceneName[digitStartIndex]))
            return false;

        int digitEndIndex = digitStartIndex;
        while (digitEndIndex < sceneName.Length && char.IsDigit(sceneName[digitEndIndex]))
            digitEndIndex++;

        string stageNumberText = sceneName.Substring(digitStartIndex, digitEndIndex - digitStartIndex);
        return int.TryParse(stageNumberText, out stageNumber);
    }

    void ShuffleAndPick()
    {
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            int temp = numbers[i];
            numbers[i] = numbers[rnd];
            numbers[rnd] = temp;
        }
    }
}
