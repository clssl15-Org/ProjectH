using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManaer : MonoBehaviour
{
    public static SpawnManaer Instance { get; private set; }

    public List<GameObject> SpawnerList { get; private set; } = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 외부에서 스포너를 추가할 수 있는 메서드
    public void AddSpawner(GameObject spawner)
    {
        if (spawner != null && !SpawnerList.Contains(spawner))
        {
            SpawnerList.Add(spawner);
        }
    }
}