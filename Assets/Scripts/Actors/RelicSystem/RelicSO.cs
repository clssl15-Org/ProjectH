using UnityEngine;

[CreateAssetMenu(fileName = "New Relic", menuName = "Project H/RelicData")]
public class RelicDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private int relicNumber;     // 넘버
    [SerializeField] private Sprite icon;            // 스프라이트
    [SerializeField] private string relicName;    // 유물 이름

    [TextArea]
    [SerializeField] private string description;     // 설명
    [TextArea]
    [SerializeField] private string nomalEffect;    // 일반 효과 설명
    [TextArea]
    [SerializeField] private string upgradeEffect; // 강화 설명

    [Header("수치 정보")]
    [SerializeField] private float baseValue;        // 기본 value (예: 데미지 증가량 0.1f)
    [SerializeField] private float coinFlipValue;    // 동전 앞면 버프 value

    [Header("설정")]
    [SerializeField] private bool canStack;          // 중복 가능 여부
    [SerializeField] private int maxStackCount = 1;  // 최대 중첩 개수

    // --- 외부 접근용 프로퍼티 (Getter) ---
    public int RelicNumber => relicNumber;
    public Sprite Icon => icon;
    public string RelicName => relicName;
    public string Description => description;
    public string NomalEffect => nomalEffect;
    public string UpgradeEffect => upgradeEffect;
    public float BaseValue => baseValue;
    public float CoinFlipValue => coinFlipValue;
    public bool CanStack => canStack;
    public int MaxStackCount => maxStackCount;
}