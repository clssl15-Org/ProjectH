using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlatformDetector), typeof(MonsterHitted))]
public abstract class Monster : MonoBehaviour
{
    // Front
    public int HP
    {
        get => hp;
        protected set => hp = Mathf.Clamp(value, 0, maxHp);
    }

    // Property 
    [Header("Parameters")]
    [SerializeField, Min(0)] private int maxHp;
    [SerializeField, Min(0)] private int attackPower;
    [SerializeField, Min(0)] private int moveSpeed;

    [Header("Bindings")]
    [SerializeField] private PlatformManager platformManager;

    // Internal
    protected Rigidbody2D Rigidbody { get; private set; }
    protected int BelongingPlatform { get; set; } = 1;
    protected PlatformDetector PlatformDetector { get; private set; }

    private int hp;

    private MonsterHitted hitDetector;
    private MonsterPlayerDetector playerDetector;


    // Content
    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();

        if (!platformManager)
        {
            throw new InvalidOperationException(
                $"platformManager가 등록되어 있지 않기 때문에 몬스터 '{name}'을(를) 시작할 수 없습니다.");
        }

        PlatformDetector = GetComponent<PlatformDetector>();
        PlatformDetector.SetPlatformManager(platformManager);

        playerDetector = GetComponentInChildren<MonsterPlayerDetector>();

        if (playerDetector)
            playerDetector.OnPlayerDetected += OnPlayerDetected;
        else
            Debug.LogWarning(
                $"이 몬스터({name})은(는) {nameof(MonsterPlayerDetector)}를 가지고 있지 않습니다.\n" +
                "플레이어 감지 기능이 정상적으로 작동하지 않을 수 있습니다.");

        hitDetector = GetComponent<MonsterHitted>();
        hitDetector.OnTakeDamage += OnDamaged;

        hp = maxHp;
    }

    protected abstract void OnPlayerDetected(GameObject player);
    protected abstract void OnDamaged(int damage);

    protected virtual void OnDestroy()
    {
        if (hitDetector)
            hitDetector.OnTakeDamage -= OnDamaged;

        if (playerDetector)
            playerDetector.OnPlayerDetected -= OnPlayerDetected;
    }
}
