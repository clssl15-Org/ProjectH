using System;
using System.Text;
using UnityEngine;

public partial class Dokkaebi : Monster
{
    // Property
    [Header("Dokkaebi")]
    [SerializeField] private GameObject laserPrefab;

    // Inspector
    [SerializeField, TextArea(3, 10)]
    private string stateDisplay = string.Empty;
    private readonly StringBuilder sb = new();
    
    // Internal
    private Brain brain;


    // Content
    protected override void Awake()
    {
        base.Awake();

        if (!laserPrefab)
        {
            throw new InvalidOperationException(
               $"laserPrefab이 등록되어 있지 않기 때문에 도깨비 '{name}'을(를) 시작할 수 없습니다.");
        }
    }

    protected void Start()
    {
        Direction = UnityEngine.Random.Range(0, 2) == 0
            ? Direction.Left
            : Direction.Right;

        brain = new(this);
        brain.Open();
    }


    private void Update()
    {
        brain?.Invoke();
        UpdateStateDisplay();
    }

    private void UpdateStateDisplay()
    {
        sb.Clear();
        sb.AppendLine($"HP: {HP}");
        sb.AppendLine($"Direction: {Direction.ToString()}");
        sb.AppendLine($"Current Platform: {(BelongingPlatform >= 0 ? BelongingPlatform : "null")}");

        if (brain is not null)
            sb.AppendLine($"State: {brain.GetFullState()}");

        stateDisplay = sb.ToString();
    }

    protected override void OnDamaged(int damage) => brain.TakeDamage(damage);

    protected override void OnPlayerDetected(GameObject player)
    {
        print($"플레이어 감지: {player.name}");
    }


    protected override void OnDestroy()
    {
        brain?.Dispose();
        base.OnDestroy();
    }
}
