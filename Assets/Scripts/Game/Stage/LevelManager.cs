using System.Collections;
using System.Collections.Generic;
using Actors;
using Actors.PlayerSystem;
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
        if (_isInitialized)
        {
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
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSpawnManagerForScene(scene);
        SyncCurrentStageFromScene(scene);
    }

    /// <summary>
    /// DDOL LevelManager와 별도로 씬마다 둔 SpawnManager(WaveDataList 등)를 현재 씬 기준으로 다시 연결합니다.
    /// sceneLoaded는 해당 씬의 Start 호출 전에 불리므로 MonsterSpawner 등록 시점과 맞습니다.
    /// </summary>
    private void RebindSpawnManagerForScene(Scene scene)
    {
        SpawnManager inScene = FindSpawnManagerInScene(scene);
        SpawnManager next = inScene != null ? inScene : _spawnManagerOnLevelRoot;

        if (next == null)
        {
            Debug.LogWarning("SpawnManager를 찾을 수 없습니다. 씬 또는 LevelManager 오브젝트에 배치해 주세요.", this);
            return;
        }

        if (_spawnManager != null && _spawnManager != next)
            _spawnManager.Clear();

        _spawnManager = next;
        _spawnManager.RefreshClearObjectForLoadedScene();
    }

    private static SpawnManager FindSpawnManagerInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return null;

        foreach (var root in scene.GetRootGameObjects())
        {
            var found = root.GetComponentsInChildren<SpawnManager>(true);
            if (found.Length == 0)
                continue;
            if (found.Length > 1)
                Debug.LogWarning($"씬 '{scene.name}'에 SpawnManager가 {found.Length}개 있습니다. 첫 번째만 사용합니다.", found[0]);
            return found[0];
        }

        return null;
    }

    public void ResetState()
    {
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
        PlayerHasDied = true;
        IsPlayerDeathRestartPending = true;
    }
    public bool ConsumePlayerDeathRestart()
    {
        if (!IsPlayerDeathRestartPending)
            return false;

        IsPlayerDeathRestartPending = false;
        return true;
    }
    public void MoveNextLevel(LevelType nextLevelType)
    {
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
