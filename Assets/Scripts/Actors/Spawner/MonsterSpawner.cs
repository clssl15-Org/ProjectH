using System;
using System.Collections;
using System.Collections.Generic;
using Actors.Monsters;
using Actors.Monsters.Brains;
using Infrastructure;
using UnityEngine;
using World;

namespace Actors
{
    public class MonsterSpawner : MonoBehaviour,
        IInjectable<GameAssetLibrary>,
        IInjectable<Configuration>
    {
        [Header("스폰 위치 설정")]
        [SerializeField]
        private List<Transform> spawnPoint;

        [Header("페이즈 설정")]
        [SerializeField]
        private bool isLargeMapWave = false;
        [SerializeField]
        private List<SpawnPhase> phases = new List<SpawnPhase>();

        [Header("마지막 소형맵 강화 스폰 설정")]
        [SerializeField]
        private bool enableEnhancedFinalSmallMapPhase = true;
        [SerializeField]
        private int enhancedExploreOrder = 5;
        [SerializeField]
        private SpawnPhase enhancedSpawnPhaseTemplate;
        [SerializeField]
        private List<MonsterTierUpgradeMapping> enhancedTierUpgradeMappings = new List<MonsterTierUpgradeMapping>();

        [Header("인디케이터 설정")]
        [SerializeField]
        private GameObject monsterSpawnIndicator;

        [Header("완료 감시 설정")]
        [SerializeField, Min(0.1f)]
        private float activeMonsterCheckInterval = 0.5f;

        public GameAssetLibrary gameAssetLibrary;
        public PlatformManager platformManager;
        public Configuration configuration;

        public bool IsAllPhasesComplete { get; private set; } = false;

        private int currentPhaseIndex = -1; // 현재 진행 중인 페이즈의 인덱스
        private bool isSpawning = false; // 스포너가 현재 작동 중인지 여부

        private List<GameObject> activeMonsters = new List<GameObject>();
        private List<Transform> tmpSpawnPoint = new List<Transform>();
        private List<SpawnPhase> runtimePhases = new List<SpawnPhase>();
        private Dictionary<MonsterTier, List<GameObject>> tierCandidates = new Dictionary<MonsterTier, List<GameObject>>();

        private Action<IMonster> monsterCreated;
        private float activeMonsterCheckTimer;
        private bool isPhaseAdvanceQueued;

        private enum MonsterTier
        {
            Unknown,
            BaseMelee,
            BaseRanged,
            SpecialMelee,
            SpecialRanged,
            SpecialStrong
        }

        private void Start()
        {
            if (!platformManager)
                Debug.LogWarning("platformManager이(가) 유효하지 않습니다", this);

            RegisterToSpawnManager();
            StartSpawner();
        }

        private void Update()
        {
            if (!isSpawning || IsAllPhasesComplete || isPhaseAdvanceQueued)
                return;

            activeMonsterCheckTimer += Time.deltaTime;
            if (activeMonsterCheckTimer < Mathf.Max(0.1f, activeMonsterCheckInterval))
                return;

            activeMonsterCheckTimer = 0f;
            CheckActiveMonstersCleared();
        }

        public void Inject(GameAssetLibrary gameAssetLibrary) =>
            this.gameAssetLibrary = gameAssetLibrary;

        public void Inject(Configuration configuration) =>
            this.configuration = configuration;

        /// <summary>
        /// 스포너 매니저에 자신을 등록합니다.
        /// </summary>
        public void RegisterToSpawnManager()
        {
            if (LevelManager.Instance == null)
            {
                Debug.LogError("LevelManager.Instance가 null이기 때문에 MonsterSpawner를 등록할 수 없습니다.", this);
                return;
            }
            if (LevelManager.Instance.SpawnManager == null)
            {
                Debug.LogError("SpawnManager가 null이기 때문에 MonsterSpawner를 등록할 수 없습니다.", this);
                return;
            }

            LevelManager.Instance.SpawnManager.AddSpawner(this);
        }

        public void OnMonsterCreate(Action<IMonster> monsterCreated)
        {
            this.monsterCreated = monsterCreated;
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
                CompleteWithoutSpawning("No spawn phases are configured");
                return;
            }

            BuildRuntimePhases();
            if (runtimePhases.Count == 0)
            {
                CompleteWithoutSpawning("No runnable spawn phases are available");
                return;
            }

            isSpawning = true;
            activeMonsterCheckTimer = 0f;
            isPhaseAdvanceQueued = false;
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
            if (currentPhaseIndex >= runtimePhases.Count)
            {
                OnAllPhasesComplete();
                return;
            }

            StartPhase(runtimePhases[currentPhaseIndex]);
        }

        private void BuildRuntimePhases()
        {
            runtimePhases.Clear();
            foreach (SpawnPhase phase in phases)
            {
                SpawnPhase runtimePhase = ClonePhase(phase);
                if (runtimePhase != null)
                    runtimePhases.Add(runtimePhase);
                else
                    Debug.LogWarning($"[{gameObject.name}] Ignored a null spawn phase.", this);
            }

            if (!ShouldUseEnhancedFinalSmallMapPhase())
            {
                return;
            }

            BuildTierCandidates();
            for (int i = 0; i < runtimePhases.Count; i++)
            {
                ApplyTierUpgradeMappings(runtimePhases[i]);
            }
        }

        private void BuildTierCandidates()
        {
            tierCandidates.Clear();
            for (int i = 0; i < runtimePhases.Count; i++)
            {
                AddTierCandidatesFromPhase(runtimePhases[i]);
            }
        }

        private void AddTierCandidatesFromPhase(SpawnPhase phase)
        {
            if (phase == null)
            {
                return;
            }

            if (phase.fixedMonsterPool != null)
            {
                for (int i = 0; i < phase.fixedMonsterPool.Count; i++)
                {
                    TryAddTierCandidate(phase.fixedMonsterPool[i]);
                }
            }

            if (phase.randomMonsterPool?.monsterPrefabs != null)
            {
                for (int i = 0; i < phase.randomMonsterPool.monsterPrefabs.Count; i++)
                {
                    TryAddTierCandidate(phase.randomMonsterPool.monsterPrefabs[i]);
                }
            }
        }

        private void TryAddTierCandidate(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            if (!TryResolveTier(prefab, out MonsterTier tier) || tier == MonsterTier.Unknown)
            {
                return;
            }

            if (!tierCandidates.TryGetValue(tier, out List<GameObject> list))
            {
                list = new List<GameObject>();
                tierCandidates.Add(tier, list);
            }

            if (!list.Contains(prefab))
            {
                list.Add(prefab);
            }
        }

        private bool ShouldUseEnhancedFinalSmallMapPhase()
        {
            if (!enableEnhancedFinalSmallMapPhase || isLargeMapWave)
            {
                return false;
            }

            if (LevelManager.Instance == null)
            {
                return false;
            }

            return LevelManager.Instance.ExploreCount == enhancedExploreOrder;
        }

        private SpawnPhase ClonePhase(SpawnPhase source)
        {
            if (source == null)
            {
                return null;
            }

            SpawnPhase clone = new SpawnPhase
            {
                phaseNumber = source.phaseNumber,
                fixedMonsterPool = source.fixedMonsterPool != null
                    ? new List<GameObject>(source.fixedMonsterPool)
                    : new List<GameObject>(),
                randomMonsterPool = CloneRandomPool(source.randomMonsterPool)
            };
            return clone;
        }

        private RandomPoolSettings CloneRandomPool(RandomPoolSettings source)
        {
            if (source == null)
            {
                return null;
            }

            RandomPoolSettings clone = new RandomPoolSettings
            {
                spawnCount = source.spawnCount,
                canDuplicate = source.canDuplicate,
                monsterPrefabs = source.monsterPrefabs != null
                    ? new List<GameObject>(source.monsterPrefabs)
                    : new List<GameObject>()
            };
            return clone;
        }

        private void ApplyTierUpgradeMappings(SpawnPhase phase)
        {
            if (phase == null)
            {
                return;
            }

            if (phase.fixedMonsterPool != null)
            {
                for (int i = 0; i < phase.fixedMonsterPool.Count; i++)
                {
                    phase.fixedMonsterPool[i] = GetUpgradedPrefab(phase.fixedMonsterPool[i]);
                }
            }

            if (phase.randomMonsterPool?.monsterPrefabs != null)
            {
                for (int i = 0; i < phase.randomMonsterPool.monsterPrefabs.Count; i++)
                {
                    phase.randomMonsterPool.monsterPrefabs[i] = GetUpgradedPrefab(phase.randomMonsterPool.monsterPrefabs[i]);
                }
            }
        }

        private GameObject GetUpgradedPrefab(GameObject original)
        {
            if (original == null)
            {
                return null;
            }

            if (enhancedTierUpgradeMappings != null)
            {
                for (int i = 0; i < enhancedTierUpgradeMappings.Count; i++)
                {
                    MonsterTierUpgradeMapping map = enhancedTierUpgradeMappings[i];
                    if (map == null || map.fromPrefab == null || map.toPrefab == null)
                    {
                        continue;
                    }

                    if (map.fromPrefab == original)
                    {
                        return map.toPrefab;
                    }
                }
            }

            if (!TryResolveTier(original, out MonsterTier sourceTier))
            {
                return original;
            }

            MonsterTier targetTier = sourceTier switch
            {
                MonsterTier.BaseMelee => MonsterTier.SpecialMelee,
                MonsterTier.BaseRanged => MonsterTier.SpecialRanged,
                MonsterTier.SpecialMelee => MonsterTier.SpecialStrong,
                MonsterTier.SpecialRanged => MonsterTier.SpecialStrong,
                _ => MonsterTier.Unknown
            };

            if (targetTier == MonsterTier.Unknown)
            {
                return original;
            }

            if (!tierCandidates.TryGetValue(targetTier, out List<GameObject> candidates) || candidates.Count == 0)
            {
                return original;
            }

            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[randomIndex] != null ? candidates[randomIndex] : original;
        }

        private bool TryResolveTier(GameObject prefab, out MonsterTier tier)
        {
            tier = MonsterTier.Unknown;
            if (prefab == null)
            {
                return false;
            }

            int stage = LevelManager.Instance != null ? LevelManager.Instance.CurrentStage : -1;
            if (stage <= 0)
            {
                return false;
            }

            string name = prefab.name.ToLowerInvariant();
            if (stage == 1)
            {
                if (ContainsAny(name, "dynastid", "stag beetle"))
                {
                    tier = MonsterTier.BaseMelee;
                    return true;
                }
                if (ContainsAny(name, "javelin hurler"))
                {
                    tier = MonsterTier.BaseRanged;
                    return true;
                }
                if (ContainsAny(name, "snail") && !ContainsAny(name, "spike"))
                {
                    tier = MonsterTier.SpecialMelee;
                    return true;
                }
                if (ContainsAny(name, "spike snail"))
                {
                    tier = MonsterTier.SpecialRanged;
                    return true;
                }
                if (ContainsAny(name, "mad wood"))
                {
                    tier = MonsterTier.SpecialStrong;
                    return true;
                }
            }
            else if (stage == 2)
            {
                if (ContainsAny(name, "blue monster"))
                {
                    tier = MonsterTier.BaseMelee;
                    return true;
                }
                if (ContainsAny(name, "crow"))
                {
                    tier = MonsterTier.BaseRanged;
                    return true;
                }
                if (ContainsAny(name, "fire imp"))
                {
                    tier = MonsterTier.SpecialMelee;
                    return true;
                }
                if (ContainsAny(name, "ghost"))
                {
                    tier = MonsterTier.SpecialRanged;
                    return true;
                }
                if (ContainsAny(name, "dark monster"))
                {
                    tier = MonsterTier.SpecialStrong;
                    return true;
                }
            }
            else if (stage == 3)
            {
                if (ContainsAny(name, "melee skeleton"))
                {
                    tier = MonsterTier.BaseMelee;
                    return true;
                }
                if (ContainsAny(name, "ranged skeleton"))
                {
                    tier = MonsterTier.BaseRanged;
                    return true;
                }
                if (ContainsAny(name, "eyeball"))
                {
                    tier = MonsterTier.SpecialMelee;
                    return true;
                }
                if (ContainsAny(name, "dokkaebi"))
                {
                    tier = MonsterTier.SpecialRanged;
                    return true;
                }
                if (ContainsAny(name, "fire monster", "dark monster", "skeleton pig"))
                {
                    tier = MonsterTier.SpecialStrong;
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsAny(string source, params string[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (source.Contains(tokens[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 지정된 페이즈 데이터를 기반으로 몬스터 스폰을 시작합니다.
        /// </summary>
        private void StartPhase(SpawnPhase phase)
        {
            if (phase == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Phase {currentPhaseIndex + 1} is null. Skipping to next phase.", this);
                QueueStartNextPhase();
                return;
            }

            // 다음 페이즈 시작 전, 추적 리스트 초기화
            activeMonsterCheckTimer = 0f;
            isPhaseAdvanceQueued = false;
            activeMonsters.Clear();

            // 임시 스폰 위치 리스트 초기화
            tmpSpawnPoint = spawnPoint != null
                ? new List<Transform>(spawnPoint)
                : new List<Transform>();

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
                QueueStartNextPhase();
            }
        }

        private void QueueStartNextPhase()
        {
            if (!isSpawning || IsAllPhasesComplete || isPhaseAdvanceQueued)
                return;

            isPhaseAdvanceQueued = true;
            StartCoroutine(WaitAndStartNextPhase());
        }

        private IEnumerator WaitAndStartNextPhase()
        {
            yield return new WaitForSeconds(1);
            isPhaseAdvanceQueued = false;
            StartNextPhase();
        }

        /// <summary>
        /// 모든 페이즈가 성공적으로 완료되었을 때 호출됩니다.
        /// </summary>
        private void OnAllPhasesComplete()
        {
            if (IsAllPhasesComplete)
                return;

            Debug.Log($"*** [{gameObject.name}] 모든 스폰 페이즈를 완료했습니다. ***");
            isSpawning = false;
            isPhaseAdvanceQueued = false;

            IsAllPhasesComplete = true;
            var spawnManager = LevelManager.Instance?.SpawnManager;
            if (spawnManager != null)
            {
                spawnManager.CheckAllSpawnersComplete();
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] SpawnManager is missing, so stage clear completion could not be checked.", this);
            }

            if (isLargeMapWave && spawnManager != null)
            {
                spawnManager.WaveComplete(this);
            }
        }

        private void CompleteWithoutSpawning(string reason)
        {
            Debug.LogWarning(
                $"[{gameObject.name}] {reason}. Marking this spawner complete so ClearObjects is not blocked.",
                this);

            OnAllPhasesComplete();
        }

        /// <summary>
        /// 고정 스폰 풀의 모든 몬스터를 스폰합니다. 
        /// </summary>
        private void ProcessFixedPool(List<GameObject> pool)
        {
            if (pool == null || pool.Count == 0) return;

            // 대형맵의 특정 몬스터 풀을 위함
            if (pool[0].TryGetComponent<monsterBundle>(out monsterBundle mb))
            {
                pool = mb.monsterList;
            }

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
                int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
                GameObject monsterPrefab = candidates[randomIndex];

                if (monsterPrefab != null)
                {
                    // stage3의 특수 강력 몬스터 풀을 위한 코드
                    if (monsterPrefab.TryGetComponent<monsterBundle>(out monsterBundle mb))
                    {
                        int randIndex = UnityEngine.Random.Range(0, mb.monsterList.Count);
                        monsterPrefab = mb.monsterList[randIndex];
                    }

                    SpawnMonster(monsterPrefab);

                    if (!pool.canDuplicate)
                    {
                        // 중복 불가 시, 선택된 몬스터를 후보 리스트에서 제거 
                        candidates.RemoveAt(randomIndex);
                        // 후보가 더 이상 없으면 리필
                        if (candidates.Count <= 0)
                        {
                            candidates = new List<GameObject>(pool.monsterPrefabs);
                            //break;
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
            if (prefab == null)
            {
                return;
            }

            if (spawnPoint == null || spawnPoint.Count == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] 스폰 포인트가 없어 몬스터를 생성할 수 없습니다.", this);
                return;
            }

            if (tmpSpawnPoint.Count == 0)
            {
                tmpSpawnPoint = spawnPoint != null
                    ? new List<Transform>(spawnPoint)
                    : new List<Transform>();
            }

            tmpSpawnPoint.RemoveAll(point => !point);

            if (tmpSpawnPoint.Count == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] 유효한 임시 스폰 포인트가 없어 몬스터를 생성할 수 없습니다.", this);
                return;
            }

            // 스폰 위치 무작위 선택 및 제거
            Transform spawnTransform = tmpSpawnPoint[UnityEngine.Random.Range(0, tmpSpawnPoint.Count)];
            tmpSpawnPoint.Remove(spawnTransform);

            // 스포너의 위치와 회전값으로 몬스터를 스폰합니다. 
            GameObject monsterInstance = Instantiate(prefab, spawnTransform.position, this.transform.rotation);
            if (!monsterInstance.TryGetComponent(out IMonster monsterScript))
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

            monsterScript.Initialize(gameAssetLibrary, configuration, platformManager);
            monsterCreated?.Invoke(monsterScript);

            // 1. 활성 몬스터 리스트에 추가하여 추적 시작 
            activeMonsters.Add(monsterInstance);

            // 2. 스폰된 몬스터의 Monster 스크립트에서 사망 이벤트를 가져옴
            RegisterRemoval(monsterScript);
            void RegisterRemoval(IMonster monster)
            {
                monster.ConditionChanged += cond =>
                {
                    if (!cond.Is(MonsterCondition.Dying, MonsterCondition.Died))
                    {
                        return;
                    }

                    // 분열/소환형 몬스터는 Dying 이벤트 payload로 자식을 전달합니다.
                    if (cond.Payload is IEnumerable<IMonster> children)
                    {
                        foreach (var child in children)
                        {
                            activeMonsters.Add(child.gameObject);
                            RegisterRemoval(child);
                        }
                    }

                    // Dying 또는 Died 중 어느 이벤트를 발행하든 동일하게 사망 처리
                    OnMonsterDied(monster.gameObject);
                };
            }

            // 스폰 인디케이터 표시
            if (monsterSpawnIndicator != null && currentPhaseIndex != 0)
            {
                GameObject indicator = Instantiate(monsterSpawnIndicator, spawnTransform.position, Quaternion.identity);
            }
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

                CheckActiveMonstersCleared();
            }
        }

        private void CheckActiveMonstersCleared()
        {
            RemoveInactiveMonsters();

            if (isSpawning && activeMonsters.Count == 0)
                QueueStartNextPhase();
        }

        private void RemoveInactiveMonsters()
        {
            activeMonsters.RemoveAll(IsInactiveMonster);
        }

        private static bool IsInactiveMonster(GameObject monsterObject)
        {
            if (!monsterObject)
                return true;

            if (!monsterObject.TryGetComponent<IMonster>(out var monster))
                return true;

            return !monster.IsAlive;
        }
    }
}
