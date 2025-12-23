using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Actors
{
    public class SpawnManaer : MonoBehaviour
    {
        public static SpawnManaer Instance { get; private set; }

        public List<GameObject> SpawnerList { get; private set; } = new List<GameObject>();

        private Action<IMonster> monsterCreated;

        private void Awake() => Instance = this;

        // 외부에서 스포너를 추가할 수 있는 메서드
        public void AddSpawner(GameObject spawner)
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
                spawner.GetComponent<MonsterSpawner>().OnMonsterCreate(monsterCreated);
        }

        /// <summary>
        /// 맵이 바뀌기 전에 이 메서드를 호출하여 MonsterSpawner 리스트를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            SpawnerList.Clear();
        }
    }
}
