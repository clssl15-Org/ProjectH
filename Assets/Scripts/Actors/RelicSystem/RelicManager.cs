using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Actors.PlayerSystem;
using System;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance { get; private set; }

    public Player player;

    [Header("Relic Prefab Registry")]
    // 모든 유물 프리팹을 인스펙터에서 등록합니다.
    [SerializeField] private List<GameObject> relicPrefabs;

    public event Action<RelicDataSO> RelicAcquiring;
    public event Action<RelicDataSO, string> RelicAcquired;

    // 현재 플레이어가 소유한 유물 오브젝트들 (Key: RelicNumber)
    private Dictionary<int, List<GameObject>> ownedRelics = new Dictionary<int, List<GameObject>>();
    public IReadOnlyDictionary<int, List<GameObject>> OwnedRelics => ownedRelics;

    private readonly float[] skillArtifactProbs = { 33.333f, 25.0f, 40.0f, 0f };
    private readonly float[] normalArtifactProbs = { 0f, 3.846f, 4.615f, 7.692f };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 이미 존재한다면 새로 생성된 객체 삭제
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지
    }

    private void Start()
    {
        if (!player)
        {
            player = FindObjectOfType<Player>();
        }
    }

    // --- Id로 유물 데이터 가져오기 ---
    public bool TryGetRelicData(int id, out RelicDataSO relicData)
    {
        relicData = relicPrefabs
            .Select(p => p.GetComponent<Relic>().Data)
            .FirstOrDefault(rd => rd.RelicNumber == id);

        return relicData != null;
    }

    public void GetRandomRelicData()
    {
        // 1. 현재 보유한 '스킬 유물(ID 1~3)' 개수 계산
        int currentSkillCount = GetSkillRelicCount();

        // 2. 상태 인덱스 클램핑 (0 ~ 3)
        int stateIndex = Mathf.Clamp(currentSkillCount, 0, 3);

        // 3. 후보군 및 가중치 계산
        Dictionary<RelicDataSO, float> candidates = new Dictionary<RelicDataSO, float>();
        float totalWeight = 0f;

        foreach (var prefab in relicPrefabs)
        {
            RelicDataSO data = prefab.GetComponent<Relic>().Data;
            int id = data.RelicNumber;

            // 이미 가지고 있고 중복 불가능하면 스킵
            if (!data.CanStack && ownedRelics.ContainsKey(id))
                continue;

            float weight = 0f;

            // ID에 따라 확률 테이블 분기 (1~3: 스킬유물 / 4~: 일반유물)
            if (id >= 1 && id <= 3)
            {
                // 스킬 유물은 이미 가지고 있으면 후보에서 제외 (중복 획득 불가 가정)
                if (ownedRelics.ContainsKey(id)) continue;
                weight = skillArtifactProbs[stateIndex];
            }
            else
            {
                // 일반 유물
                weight = normalArtifactProbs[stateIndex];
            }

            // 가중치가 0보다 클 때만 후보 등록
            if (weight > 0)
            {
                candidates.Add(data, weight);
                totalWeight += weight;
            }
        }

        // 뽑을 수 있는 유물이 없는 경우
        if (candidates.Count == 0)
        {
            Debug.Log("뽑을 수 있는 유물이 없습니다.");
            return;
        }

        // 4. 가중치 랜덤 선택 (Roulette Wheel)
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentSum = 0f;
        RelicDataSO selectedData = null;

        foreach (var kvp in candidates)
        {
            currentSum += kvp.Value;
            if (randomValue <= currentSum)
            {
                selectedData = kvp.Key;
                break;
            }
        }

        // 부동소수점 오차로 선택되지 않았을 경우 마지막 아이템 선택
        if (selectedData == null) selectedData = candidates.Last().Key;

        // 5. 결과 전달
        RelicAcquiring?.Invoke(selectedData);
    }

    // [추가] 현재 보유한 스킬 유물(ID 1,2,3) 개수를 세는 헬퍼 함수
    private int GetSkillRelicCount()
    {
        int count = 0;
        if (ownedRelics.ContainsKey(1)) count++;
        if (ownedRelics.ContainsKey(2)) count++;
        if (ownedRelics.ContainsKey(3)) count++;
        return count;
    }

    public bool StartCoinRandom(int key)
    {
        int rnd = UnityEngine.Random.Range(0, 2);
        // 0이면 실패(false), 1이면 성공(true)
        return rnd == 1;
    }

    // --- 유물 추가 (프리팹 생성) ---
    public void AddRelic(int key, bool isReinforced = false)
    {
        // 1. 레지스트리에서 해당 번호를 가진 프리팹 찾기
        GameObject prefab = relicPrefabs.Find(p => p.GetComponent<Relic>().Data.RelicNumber == key);

        if (prefab == null)
        {
            Debug.LogWarning($"입력 키 '{key}'에 해당하는 유물을 찾지 못했습니다.");
            return;
        }

        // 2. 데이터 가져오기 및 중복 체크
        RelicDataSO data = prefab.GetComponent<Relic>().Data;

        // 중복 획득 불가인데 이미 가지고 있는 경우
        if (!data.CanStack && ownedRelics.ContainsKey(key))
        {
            Debug.LogWarning($"유물 '{data.name}'(Key:{key})은 중복 획득이 불가능합니다.");
            return;
        }

        // 3. 프리팹 생성 및 설정
        GameObject relicObj = Instantiate(prefab, this.transform);

        if (relicObj.TryGetComponent<Relic>(out var relicScript))
        {
            relicScript.isReinforced = isReinforced;

            if (!ownedRelics.ContainsKey(key)) ownedRelics[key] = new List<GameObject>();
            ownedRelics[key].Add(relicObj);

            // 획득 효과 발동
            relicScript.OnAcquire();
        }

        string description = data.Description + "\n";
        description += relicScript.isReinforced ? data.UpgradeEffect : data.NomalEffect;

        RelicAcquired?.Invoke(data, description);
    }

    // --- 유물 제거 ---
    public void RemoveRelic(int key)
    {
        // 1. 소유 여부 확인
        if (!ownedRelics.ContainsKey(key) || ownedRelics[key].Count == 0)
        {
            Debug.LogWarning($"제거하려는 유물(Key: {key})을 소유하고 있지 않습니다.");
            return;
        }

        // 2. 가장 최근에 추가된 유물 객체 가져오기 (LIFO)
        List<GameObject> relicList = ownedRelics[key];
        GameObject relicToRemove = relicList[relicList.Count - 1];

        // 3. 유물 효과 해제 호출
        Relic relicScript = relicToRemove.GetComponent<Relic>();
        if (relicScript != null)
        {
            relicScript.OnLose();
        }

        // 4. 리스트에서 제거 및 실제 객체 파괴
        relicList.RemoveAt(relicList.Count - 1);
        Destroy(relicToRemove);

        // 5. 키 삭제
        if (relicList.Count == 0)
        {
            ownedRelics.Remove(key);
        }

        Debug.Log($"유물(Key: {key}) 제거 완료.");
    }


    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 씬이 바뀌면 새로운 플레이어 오브젝트를 자동으로 할당
        //player = FindObjectOfType<Player>();
    }
}
