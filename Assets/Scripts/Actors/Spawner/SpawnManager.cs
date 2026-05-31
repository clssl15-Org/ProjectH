using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Game.Stage;

namespace Actors
{
    [Serializable]
    public struct WaveData
    {
        public MonsterSpawner spawner;
        public GameObject waveObject;
    }

    public class SpawnManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject clearObject;

        [SerializeField]
        public List<WaveData> waveDataList = new List<WaveData>();

        public List<MonsterSpawner> SpawnerList { get; private set; } = new List<MonsterSpawner>();

        private Action<IMonster> monsterCreated;
        public void AddSpawner(MonsterSpawner spawner)
        {
            if (spawner != null && !SpawnerList.Contains(spawner))
            {
                spawner.GetComponent<MonsterSpawner>().OnMonsterCreate(monsterCreated);
                SpawnerList.Add(spawner);
            }
        }

        public void OnMonsterCreate(Action<IMonster> monsterCreated)
        {
            this.monsterCreated = monsterCreated;

            foreach (var spawner in SpawnerList)
                spawner.OnMonsterCreate(monsterCreated);
        }

        public void CheckAllSpawnersComplete()
        {
            if (SpawnerList.Count == 0) return;

            foreach (var spawner in SpawnerList)
            {
                if (!spawner.IsAllPhasesComplete)
                {
                    return;
                }
            }

            OnAllMonstersCleared();
        }

        private void OnAllMonstersCleared()
        {
            ClearObjectsActivator.ActivateRootAndChildren(clearObject);
        }
        public void WaveComplete(MonsterSpawner spawner)
        {
            if (spawner == null)
            {
                Debug.LogWarning("WaveComplete called with null spawner.", this);
                return;
            }

            bool matchedByReference = false;
            foreach (var waveData in waveDataList)
            {
                if (waveData.spawner == spawner)
                {
                    matchedByReference = true;
                    if (waveData.waveObject != null)
                    {
                        waveData.waveObject.SetActive(true);
                    }
                    else
                    {
                        Debug.LogWarning($"Wave object is null for spawner: {spawner.name}", this);
                    }
                    return;
                }
            }

            for (int i = 0; i < waveDataList.Count; i++)
            {
                var waveData = waveDataList[i];
                if (waveData.spawner == null || waveData.waveObject == null)
                    continue;

                if (waveData.spawner.name == spawner.name)
                {
                    waveData.waveObject.SetActive(true);
                    Debug.LogWarning($"WaveData reference mismatch. Used name fallback for spawner: {spawner.name}", this);
                    return;
                }
            }

            int spawnerIndex = SpawnerList.IndexOf(spawner);
            if (spawnerIndex >= 0 && spawnerIndex < waveDataList.Count)
            {
                var fallbackData = waveDataList[spawnerIndex];
                if (fallbackData.waveObject != null)
                {
                    fallbackData.waveObject.SetActive(true);
                    Debug.LogWarning($"WaveData reference mismatch. Used index fallback for spawner: {spawner.name} (index: {spawnerIndex})", this);
                    return;
                }
            }

            if (!matchedByReference)
                Debug.LogWarning($"WaveComplete match failed for spawner: {spawner.name}. Check waveDataList mapping.", this);
        }

        public void Clear()
        {
            monsterCreated = null;
            SpawnerList.Clear();
        }

        /// <summary>
        /// 씬에 배치된 SpawnManager의 waveDataList 등 씬 전용 설정을 DDOL 인스턴스로 복사합니다.
        /// </summary>
        public void ApplyConfigurationFrom(SpawnManager source)
        {
            if (source == null || source == this)
                return;

            waveDataList = source.waveDataList != null
                ? new List<WaveData>(source.waveDataList)
                : new List<WaveData>();
        }

        public void RefreshClearObjectForLoadedScene(Scene scene)
        {
            GameObject found = FindClearObjectsInScene(scene);
            if (found != null)
                clearObject = found;

            if (clearObject == null)
                return;

            if (LevelManager.Instance != null && LevelManager.Instance.ExploreCount > 0)
                clearObject.SetActive(false);
        }

        private static GameObject FindClearObjectsInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return null;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "ClearObjects")
                        return child.gameObject;
                }
            }

            return null;
        }
    }
}