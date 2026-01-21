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
    //public event Action<RelicDataSO> RelicAcquired;
    public event Action<(RelicDataSO, string)> RelicAcquired;

    // 현재 플레이어가 소유한 유물 오브젝트들 (Key: RelicNumber)
    private Dictionary<int, List<GameObject>> ownedRelics = new Dictionary<int, List<GameObject>>();
    public IReadOnlyDictionary<int, List<GameObject>> OwnedRelics => ownedRelics;

    private void Awake()
    {
        // --- DontDestroyOnLoad 및 싱글톤 중복 방지 로직 ---
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


    // --- 랜덤으로 유물 데이터 뽑기 (UI용) ---
    public void GetRandomRelicData()
    {
        // 프리팹에 붙어있는 Relic 컴포넌트에서 데이터를 읽어와 필터링합니다.
        var available = relicPrefabs
            .Select(p => p.GetComponent<Relic>().Data)
            .Where(d => d.CanStack || !ownedRelics.ContainsKey(d.RelicNumber))
            .ToList();

        if (available.Count == 0) return;
        RelicAcquiring?.Invoke(available[UnityEngine.Random.Range(0, available.Count)]);
    }

    public bool StartCoinRandom(int key)
    {
        int rnd = UnityEngine.Random.Range(0, 2);
        bool canReinforced = false;

        if (rnd == 0)
        {
            // 실패
            canReinforced = false;
        }
        else if (rnd == 1)
        {
            // 성공
            canReinforced = true;
        }

        return canReinforced;
    }


    // --- 유물 추가 (프리팹 생성) ---
    public void AddRelic(int key, bool isReinforced = false)
    {
        // 1. 레지스트리에서 해당 번호를 가진 프리팹 찾기
        GameObject prefab = relicPrefabs.Find(p => p.GetComponent<Relic>().Data.RelicNumber == key);

        // 예외 안내 메세지 추가
        if (prefab == null)
        {
            Debug.LogWarning(
                $"입력 키 '{key}'에 해당하는 {nameof(Relic)}을(를) 찾는 데 실패했습니다. " +
                $"렐릭을 추가하지 않습니다.");

            return;
        }

        // 2. 데이터 가져오기 및 중복 체크
        RelicDataSO data = prefab.GetComponent<Relic>().Data;
        if (!data.CanStack && ownedRelics.ContainsKey(key))
        {
            Debug.LogWarning(
                $"입력 키 '{key}'에 해당하는 {nameof(RelicDataSO)}을(를) 찾는 데 실패했습니다. " +
                $"렐릭을 추가하지 않습니다.");

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

        RelicAcquired?.Invoke(data);
    }

    // --- 유물 제거 ---
    public void RemoveRelic(int key)
    {
        // 1. 소유 여부 확인
        if (!ownedRelics.ContainsKey(key) || ownedRelics[key].Count == 0)
        {
            Debug.LogWarning($"제거하려는 유물(Key: {key})을 플레이어가 소유하고 있지 않습니다.");
            return;
        }

        // 2. 가장 최근에 추가된 유물 객체 가져오기 (리스트의 마지막 요소)
        List<GameObject> relicList = ownedRelics[key];
        GameObject relicToRemove = relicList[relicList.Count - 1];

        // 3. 유물 효과 해제 호출
        Relic relicScript = relicToRemove.GetComponent<Relic>();
        if (relicScript != null)
        {
            relicScript.OnLose(); // 유물 상실 시 발동할 로직 (스탯 감소 등)
        }

        // 4. 리스트에서 제거 및 실제 객체 파괴
        relicList.RemoveAt(relicList.Count - 1);
        Destroy(relicToRemove);

        // 5. 만약 해당 종류의 유물이 더 이상 없다면 키 삭제
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
