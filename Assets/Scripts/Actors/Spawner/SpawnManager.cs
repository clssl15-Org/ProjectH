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

        [SerializeField, Min(1)]
        private int completionWatchdogFrameInterval = 10;

        [SerializeField]
        public List<WaveData> waveDataList = new List<WaveData>();

        public List<MonsterSpawner> SpawnerList { get; private set; } = new List<MonsterSpawner>();

        private Action<IMonster> monsterCreated;

        private int completionWatchdogFrameCounter;
        private int lastSpawnerRegistrationFrame = -1;
        private bool clearObjectsActivated;
        private bool missingClearObjectWarningLogged;

        private void Update()
        {
            if (clearObjectsActivated || SpawnerList.Count == 0)
                return;

            completionWatchdogFrameCounter++;
            if (completionWatchdogFrameCounter < Mathf.Max(1, completionWatchdogFrameInterval))
                return;

            completionWatchdogFrameCounter = 0;
            CheckAllSpawnersComplete(logIncompleteSpawners: false);
        }

        public void AddSpawner(MonsterSpawner spawner)
        {
            if (spawner != null && !SpawnerList.Contains(spawner))
            {
                spawner.OnMonsterCreate(monsterCreated);
                SpawnerList.Add(spawner);
                completionWatchdogFrameCounter = 0;
                lastSpawnerRegistrationFrame = Time.frameCount;
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
            CheckAllSpawnersComplete(logIncompleteSpawners: true);
        }

        private void CheckAllSpawnersComplete(bool logIncompleteSpawners)
        {
            RemoveInvalidSpawners();

            if (clearObjectsActivated)
                return;

            if (SpawnerList.Count == 0) return;

            List<string> incompleteSpawners = null;
            foreach (var spawner in SpawnerList)
            {
                if (!spawner.IsAllPhasesComplete)
                {
                    incompleteSpawners ??= new List<string>();
                    incompleteSpawners.Add(spawner.name);
                }
            }

            if (incompleteSpawners != null)
            {
                if (logIncompleteSpawners)
                {
                    Debug.LogWarning(
                        $"[SpawnManager] Waiting for {incompleteSpawners.Count}/{SpawnerList.Count} spawners before activating ClearObjects in scene '{SceneManager.GetActiveScene().name}': {string.Join(", ", incompleteSpawners)}",
                        this);
                }

                return;
            }

            if (IsSpawnerRegistrationSettling())
                return;

            OnAllMonstersCleared();
        }

        private void OnAllMonstersCleared()
        {
            if (clearObjectsActivated)
                return;

            ResolveClearObjectReference();
            if (!clearObject)
            {
                if (!missingClearObjectWarningLogged)
                {
                    Debug.LogWarning($"[SpawnManager] ClearObjects was not found in scene '{SceneManager.GetActiveScene().name}'.", this);
                    missingClearObjectWarningLogged = true;
                }

                return;
            }

            missingClearObjectWarningLogged = false;
            ClearObjectsActivator.ActivateRootAndChildren(clearObject);
            clearObjectsActivated = true;
        }

        private void RemoveInvalidSpawners()
        {
            int removedCount = SpawnerList.RemoveAll(spawner => !spawner);
            if (removedCount > 0)
            {
                Debug.LogWarning($"[SpawnManager] Removed {removedCount} invalid spawner references before checking stage clear.", this);
            }
        }

        private void ResolveClearObjectReference()
        {
            var found = GameObject.Find("ClearObjects");
            if (found != null)
                clearObject = found;
        }

        private bool IsSpawnerRegistrationSettling()
        {
            if (lastSpawnerRegistrationFrame < 0)
                return false;

            return Time.frameCount - lastSpawnerRegistrationFrame < Mathf.Max(1, completionWatchdogFrameInterval);
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
            completionWatchdogFrameCounter = 0;
            lastSpawnerRegistrationFrame = -1;
            clearObjectsActivated = false;
            missingClearObjectWarningLogged = false;
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
