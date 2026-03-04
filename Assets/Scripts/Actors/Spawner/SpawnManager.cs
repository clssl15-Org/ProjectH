using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Actors
{
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject clearObject;

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
            clearObject.SetActive(true);
        }

    /// <summary>
    /// 맵이 바뀌기 전에 이 메서드를 호출하여 MonsterSpawner 리스트를 초기화합니다.
    /// </summary>
    public void Clear()
        {
            monsterCreated = null;
            SpawnerList.Clear();
        }
    }
}
