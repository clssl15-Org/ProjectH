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
    public event Action<int> RelicAcquired;

    // 현재 플레이어가 소유한 유물 오브젝트들 (Key: RelicNumber)
    private Dictionary<int, List<GameObject>> _ownedRelics = new Dictionary<int, List<GameObject>>();
    public IReadOnlyDictionary<int, List<GameObject>> OwnedRelics => _ownedRelics;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (!player)
        {
            player = FindObjectOfType<Player>();
        }
    }

    // --- 랜덤으로 유물 데이터 뽑기 (UI용) ---
    public void GetRandomRelicData()
    {
        // 프리팹에 붙어있는 Relic 컴포넌트에서 데이터를 읽어와 필터링합니다.
        var available = relicPrefabs
            .Select(p => p.GetComponent<Relic>().Data)
            .Where(d => d.CanStack || !_ownedRelics.ContainsKey(d.RelicNumber))
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
    public void AddRelic(int key, bool isReinforced)
    {
        // 1. 레지스트리에서 해당 번호를 가진 프리팹 찾기
        GameObject prefab = relicPrefabs.Find(p => p.GetComponent<Relic>().Data.RelicNumber == key);

        if (prefab == null) return;

        // 2. 데이터 가져오기 및 중복 체크
        RelicDataSO data = prefab.GetComponent<Relic>().Data;
        if (!data.CanStack && _ownedRelics.ContainsKey(key)) return;

        // 3. 프리팹 생성 및 설정
        GameObject relicObj = Instantiate(prefab, this.transform);
        Relic relicScript = relicObj.GetComponent<Relic>();
        relicScript.isReinforced = isReinforced;

        if (relicScript != null)
        {
            if (!_ownedRelics.ContainsKey(key)) _ownedRelics[key] = new List<GameObject>();
            _ownedRelics[key].Add(relicObj);

            // 획득 효과 발동
            relicScript.OnAcquire();
        }

        RelicAcquired?.Invoke(key);
    }
}