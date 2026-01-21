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
    public event Action<RelicDataSO> RelicAcquired;

    // 현재 플레이어가 소유한 유물 오브젝트들 (Key: RelicNumber)
    private Dictionary<int, List<GameObject>> ownedRelics = new Dictionary<int, List<GameObject>>();
    public IReadOnlyDictionary<int, List<GameObject>> OwnedRelics => ownedRelics;

    private void Awake() => Instance = this;

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
}
