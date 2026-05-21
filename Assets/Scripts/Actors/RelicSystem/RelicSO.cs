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

    [Header("수치 정보")]
    [SerializeField] private float baseValue;        // 기본 value (예: 데미지 증가량 0.1f)
    [SerializeField] private float coinFlipValue;    // 동전 앞면 버프 value

    [Header("설정")]
    [SerializeField] private bool canStack;          // 중복 가능 여부
    [Tooltip("누적 효과 수치 상한. -1이면 제한 없음. 중복 불가 유물은 보유 여부(1개)로 판단합니다.")]
    [SerializeField] private float maxAccumulatedValue = -1f;
    [Tooltip("체크 시 HUD·유물 정보 패널에 표시하지 않습니다. (즉시 소모형 포션 등)")]
    [SerializeField] private bool hiddenFromRelicUI;

    // --- 외부 접근용 프로퍼티 (Getter) ---
    public int RelicNumber => relicNumber;
    public Sprite Icon => icon;
    public string RelicName => relicName;
    public string Description => description;
    public string NomalEffect => nomalEffect;
    public float BaseValue => baseValue;
    public float CoinFlipValue => coinFlipValue;
    public bool CanStack => canStack;
    /// <summary>누적 효과 수치 상한. &lt; 0 이면 제한 없음.</summary>
    public float MaxAccumulatedValue => maxAccumulatedValue;
    public bool HasAccumulationCap => canStack && maxAccumulatedValue >= 0f;
    public bool HiddenFromRelicUI => hiddenFromRelicUI;
}
