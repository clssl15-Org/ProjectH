using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        // 외부에서 스포너를 추가할 수 있는 메서드
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
                // 아직 완료되지 않은 스포너가 하나라도 있다면 함수 종료
                if (!spawner.IsAllPhasesComplete)
                {
                    return;
                }
            }

            OnAllMonstersCleared();
        }

        private void OnAllMonstersCleared()
        {
            if (clearObject != null)
                clearObject.SetActive(true);
        }
        public void WaveComplete(MonsterSpawner spawner)
        {
            foreach (var waveData in waveDataList)
            {
                if (waveData.spawner == spawner)
                {
                    waveData.waveObject.SetActive(true);
                    break;
                }
            }
        }

        /// <summary>
        /// 맵이 바뀌기 전에 이 메서드를 호출하여 MonsterSpawner 리스트를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            monsterCreated = null;
            SpawnerList.Clear();
        }

        /// <summary>
        /// LevelManager가 씬 로드 후 활성 SpawnManager로 리바인딩할 때 호출합니다.
        /// (DDOL 매니저만 sceneLoaded를 구독하면 씬 쪽 인스턴스의 clearObject는 갱신되지 않음)
        /// </summary>
        public void RefreshClearObjectForLoadedScene()
        {
            var found = GameObject.Find("ClearObjects");
            if (found != null)
                clearObject = found;

            if (clearObject == null)
                return;

            if (LevelManager.Instance != null && LevelManager.Instance.ExploreCount > 0)
                clearObject.SetActive(false);
        }
    }
}