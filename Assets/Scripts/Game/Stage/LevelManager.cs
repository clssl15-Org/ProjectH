using System.Collections;
using System.Collections.Generic;
using Actors;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    private static bool _isInitialized = false;
    public SpawnManager SpawnManager => _spawnManager;
    private SpawnManager _spawnManager;

    public int CurrentStage { get; private set; } = 0;

    private List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    private int exploreIndex = 0;
    private int maxExploreCount = 4;
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

        _spawnManager = GetComponent<SpawnManager>();

        ShuffleAndPick();
    }
    public void MoveNextLevel(bool stageChange)
    {
        string nextScene = default;
        if (stageChange)
        {
            CurrentStage += 1;
            exploreCount = 0;
            nextScene = $"Stage{CurrentStage} 0";
        }
        else
        {
            exploreCount += 1;
            if (exploreCount >= maxExploreCount)
            {
                nextScene = $"Stage{CurrentStage}_LargeMap";
            }
            else
            {
                exploreIndex = (exploreIndex + 1) % numbers.Count;
                nextScene = $"Stage{CurrentStage} {numbers[exploreIndex]}";
            }
        }

        LoadNextScene(nextScene);
    }
    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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
