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

    // relicData, description, forceSuccess
    public event Action<RelicDataSO, string, bool> RelicAcquiring;
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

        // Game Manager의 자식이기 때문에 씬 전환 시 파괴되지 않습니다.
        //DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴 방지
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

    public float GetValueSum(int id)
    {
        float sum = 0;

        if (!ownedRelics.ContainsKey(id) || ownedRelics[id].Count == 0)
        {
            return sum;
        }

        foreach (var relicObj in ownedRelics[id])
        {
            if (relicObj.TryGetComponent<Relic>(out var relicScript))
            {
                sum += relicScript.Value;
            }
        }
        return sum;
    }

    public void GetRandomRelicData(bool forceSuccess = false)
    {
        int currentSkillCount = GetSkillRelicCount();
        int stateIndex = Mathf.Clamp(currentSkillCount, 0, 3);

        Dictionary<RelicDataSO, float> candidates = new Dictionary<RelicDataSO, float>();
        float totalWeight = 0f;

        foreach (var prefab in relicPrefabs)
        {
            RelicDataSO data = prefab.GetComponent<Relic>().Data;
            int id = data.RelicNumber;

            // 1. 현재 보유 개수 확인
            int currentCount = 0;
            if (ownedRelics.ContainsKey(id))
            {
                currentCount = ownedRelics[id].Count;
            }

            // 2. 보유 개수가 최대 중첩 수 이상이면 후보에서 제외
            if (currentCount >= data.MaxStackCount)
            {
                continue;
            }

            // 3. 확률 적용 로직
            float weight = 0f;

            if (id >= 1 && id <= 3) // 스킬 유물
            {
                weight = skillArtifactProbs[stateIndex];
            }
            else // 일반 유물
            {
                weight = normalArtifactProbs[stateIndex];
            }

            if (weight > 0)
            {
                candidates.Add(data, weight);
                totalWeight += weight;
            }
        }

        if (candidates.Count == 0)
        {
            Debug.Log("더 이상 획득 가능한 유물이 없습니다 (모든 유물 최대치 도달).");
            return;
        }

        // 룰렛 휠 선택
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

        if (selectedData == null) selectedData = candidates.Last().Key;

        string description = selectedData.Description + "\n" + "\n";
        string effectDesc = selectedData.NomalEffect.Replace("@", selectedData.BaseValue.ToString());
        effectDesc = effectDesc.Replace("$", "");
        description += effectDesc;

        foreach (Action<RelicDataSO, string, bool> callback in RelicAcquiring.GetInvocationList())
        {
            try
            {
                callback.Invoke(selectedData, description, forceSuccess);
            }
            catch (Exception ex)
            {
                Debug.LogError($"RelicAcquiring 콜백 실패: {ex}", this);
            }
        }
    }
    // (디버그용) 선택한 렐릭 강제 추가
    public void GetRelicData(int key, bool forceSuccess = false)
    {
        var targetData = relicPrefabs
            .Select(relicObj => relicObj.GetComponent<Relic>().Data)
            .FirstOrDefault(relicData => relicData.RelicNumber == key);

        if (targetData == null)
        {
            Debug.LogWarning($"GetRelicData 실패: Key '{key}'에 해당하는 프리팹 없음.");
            return;
        }

        string description = targetData.Description + "\n" + "\n";
        string effectDesc = targetData.NomalEffect.Replace("@", targetData.BaseValue.ToString());
        effectDesc = effectDesc.Replace("$", "");
        description += effectDesc;

        RelicAcquiring?.Invoke(targetData, description, forceSuccess);
    }

    // 현재 보유한 스킬 유물(ID 1,2,3) 개수를 세는 헬퍼 함수
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
    public void AddRelic(int key, out string description, bool isReinforced = false)
    {
        description = string.Empty;
        GameObject prefab = relicPrefabs.Find(p => p.GetComponent<Relic>().Data.RelicNumber == key);

        if (prefab == null)
        {
            description = $"AddRelic 실패: Key '{key}'에 해당하는 프리팹 없음.";
            Debug.LogWarning(description);
            return;
        }

        RelicDataSO data = prefab.GetComponent<Relic>().Data;

        // 현재 보유량 체크
        int currentCount = 0;
        if (ownedRelics.ContainsKey(key))
        {
            currentCount = ownedRelics[key].Count;
        }

        // 최대 중첩 수 초과 시 추가 중단
        if (currentCount >= data.MaxStackCount)
        {
            description = $"유물 '{data.RelicName}'(Key:{key})은 최대 중첩 수({data.MaxStackCount})에 도달하여 더 이상 추가할 수 없습니다.";
            Debug.LogWarning(description);
            return;
        }

        // 생성 및 리스트 추가
        GameObject relicObj = Instantiate(prefab, this.transform);

        if (relicObj.TryGetComponent<Relic>(out var relicScript))
        {
            relicScript.isReinforced = isReinforced;

            if (!ownedRelics.ContainsKey(key)) ownedRelics[key] = new List<GameObject>();
            ownedRelics[key].Add(relicObj);

            relicScript.OnAcquire();
        }

        description = data.Description + "\n" + "\n";
        float valueSum = GetValueSum(key);
        string effectDesc = data.NomalEffect.Replace("@", valueSum.ToString());
        float added = valueSum - data.BaseValue;

        if (added > 0)
        {
            effectDesc = effectDesc.Replace("$", $"(+{added}%)");
        }
        else
        {
            effectDesc = effectDesc.Replace("$", "");
        }
        description += effectDesc;

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
