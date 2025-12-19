using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Actors.Monsters;
using Actors.Monsters.Brains;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 위치 설정")]
    [SerializeField]
    private List<Transform> spawnPoint;

    [Header("페이즈 설정")]
    [SerializeField]
    private List<SpawnPhase> phases = new List<SpawnPhase>();

    public Infrastructure.SceneAssetsLibrary sceneAssetsLibrary;
    public PlatformManager platformManager;

    private int currentPhaseIndex = -1; // 현재 진행 중인 페이즈의 인덱스
    private bool isSpawning = false; // 스포너가 현재 작동 중인지 여부

    private List<GameObject> activeMonsters = new List<GameObject>();
    private List<Transform> tmpSpawnPoint = new List<Transform>();

    private void Start()
    {
        RegisterToSpawnManager();
        StartSpawner();
    }

    /// <summary>
    /// 스포너 매니저에 자신을 등록합니다.
    /// </summary>
    public void RegisterToSpawnManager()
    {
        if(SpawnManaer.Instance != null)
        {
            SpawnManaer.Instance.AddSpawner(this.gameObject);
        }
    }
    /// <summary>
    /// 스포너 시스템 시작
    /// </summary>
    public void StartSpawner()
    {
        if (isSpawning)
        {
            Debug.LogWarning($"[{gameObject.name}] 스포너가 이미 작동 중입니다.", this);
            return;
        }

        if (phases == null || phases.Count == 0)
        {
            Debug.LogError($"[{gameObject.name}] 스폰 페이즈가 설정되지 않았습니다.", this);
            return;
        }

        isSpawning = true;
        currentPhaseIndex = -1; // StartNextPhase에서 0으로 증가하여 시작
        StartNextPhase();
    }

    /// <summary>
    /// 다음 페이즈 시작
    /// </summary>
    private void StartNextPhase()
    {
        currentPhaseIndex++;

        // 모든 페이즈가 완료되었는지 확인
        if (currentPhaseIndex >= phases.Count)
        {
            OnAllPhasesComplete();
            return;
        }

        StartPhase(phases[currentPhaseIndex]);
    }

    /// <summary>
    /// 지정된 페이즈 데이터를 기반으로 몬스터 스폰을 시작합니다.
    /// </summary>
    private void StartPhase(SpawnPhase phase)
    {
        // 다음 페이즈 시작 전, 추적 리스트 초기화
        activeMonsters.Clear();

        // 임시 스폰 위치 리스트 초기화
        tmpSpawnPoint = new List<Transform>(spawnPoint);

        // 1. 고정 스폰 풀 처리
        ProcessFixedPool(phase.fixedMonsterPool);

        // 2. 랜덤 스폰 풀 처리
        ProcessRandomPool(phase.randomMonsterPool);

        // 3. [엣지 케이스 처리]
        // 만약 페이즈에 스폰할 몬스터가 0마리라면, 즉시 다음 페이즈로 넘어갑니다.
        if (activeMonsters.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] 페이즈 {currentPhaseIndex + 1}: 스폰된 몬스터가 없어 즉시 다음 페이즈로 넘어갑니다.");
            // 한 프레임 대기 후 다음 페이즈 호출 (무한 재귀 방지)
            StartCoroutine(WaitAndStartNextPhase());
        }
    }

    private IEnumerator WaitAndStartNextPhase()
    {
        yield return new WaitForSeconds(1);
        StartNextPhase();
    }

    /// <summary>
    /// 모든 페이즈가 성공적으로 완료되었을 때 호출됩니다.
    /// </summary>
    private void OnAllPhasesComplete()
    {
        Debug.Log($"*** [{gameObject.name}] 모든 스폰 페이즈를 완료했습니다. ***");
        isSpawning = false;
    }

    /// <summary>
    /// 고정 스폰 풀의 모든 몬스터를 스폰합니다. 
    /// </summary>
    private void ProcessFixedPool(List<GameObject> pool)
    {
        if (pool == null) return;

        foreach (GameObject monsterPrefab in pool)
        {
            if (monsterPrefab != null)
            {
                SpawnMonster(monsterPrefab);
            }
        }
    }

    /// <summary>
    /// 랜덤 스폰 풀의 규칙에 따라 몬스터를 스폰합니다. [7, 12]
    /// </summary>
    private void ProcessRandomPool(RandomPoolSettings pool)
    {
        if (pool == null || pool.monsterPrefabs == null || pool.monsterPrefabs.Count == 0 || pool.spawnCount == 0)
        {
            return;
        }

        List<GameObject> candidates = new List<GameObject>(pool.monsterPrefabs);

        for (int i = 0; i < pool.spawnCount; i++)
        {
            // 후보 리스트에서 랜덤 인덱스 선택 
            int randomIndex = Random.Range(0, candidates.Count);
            GameObject monsterPrefab = candidates[randomIndex];

            if (monsterPrefab != null)
            {
                // stage3의 특수 강력 몬스터 풀을 위한 코드
                if (monsterPrefab.TryGetComponent<monsterBundle>(out monsterBundle mb))
                {
                    int randIndex = Random.Range(0, mb.monsterList.Count);
                    monsterPrefab = mb.monsterList[randIndex];
                }

                SpawnMonster(monsterPrefab);
                
                if(!pool.canDuplicate)
                {
                    // 중복 불가 시, 선택된 몬스터를 후보 리스트에서 제거 
                    candidates.RemoveAt(randomIndex);
                    // 후보가 더 이상 없으면 종료 
                    if (candidates.Count <= 0)
                    {
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 몬스터 프리팹을 인스턴스화하고, 추적 리스트에 추가하며, 사망 이벤트를 구독합니다.
    /// </summary>
    private void SpawnMonster(GameObject prefab)
    {
        // 스폰 위치 무작위 선택 및 제거
        Transform spawnTransform = tmpSpawnPoint[Random.Range(0, tmpSpawnPoint.Count)];
        tmpSpawnPoint.Remove(spawnTransform);

        // 스포너의 위치와 회전값으로 몬스터를 스폰합니다. 
        GameObject monsterInstance = Instantiate(prefab, spawnTransform.position, this.transform.rotation);
        if (!monsterInstance.TryGetComponent(out Actors.IMonster monsterScript))
        {
            var exception = new System.ArgumentException(
                $"Prefab에 IMonster 스크립트가 없습니다",
                nameof(prefab)
                );

            Debug.LogException(exception);
            return;

            //Debug.LogError($"스폰된 몬스터 '{prefab.name}'에 스크립트가 없습니다.", monsterInstance);
            //return;
        }

        monsterScript.Initialize(sceneAssetsLibrary, platformManager);

        // 1. 활성 몬스터 리스트에 추가하여 추적 시작 
        activeMonsters.Add(monsterInstance);

        // 2. 스폰된 몬스터의 Monster 스크립트에서 사망 이벤트를 가져옴
        monsterScript.ConditionChanged += cond =>
        {
            if (cond.Condition == Actors.MonsterCondition.Die)
            {
                // 죽었을 때 처리
                OnMonsterDied(monsterInstance);
            }
        };
    }

    /// <summary>
    /// 스폰된 몬스터로부터 사망 이벤트를 수신했을 때 호출되는 콜백 메서드입니다. 
    /// </summary>
    /// <param name="deadMonster">사망을 알린 몬스터의 GameObject</param>
    private void OnMonsterDied(GameObject deadMonster)
    {
        // 이 스포너가 관리하던 몬스터가 맞는지 재확인
        if (activeMonsters.Contains(deadMonster))
        {
            // 1. 추적 리스트에서 사망한 몬스터를 제거 
            activeMonsters.Remove(deadMonster);

            // 2. 활성 몬스터가 0마리가 되었는지 확인 
            if (isSpawning && activeMonsters.Count == 0)
            {
                print("test: all monsters dead");
                // 3. 현재 페이즈의 모든 몬스터가 사망했으므로, 다음 페이즈 시작 
                StartCoroutine(WaitAndStartNextPhase());
            }
        }
    }
}