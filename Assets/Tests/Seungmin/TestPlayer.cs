using System;
using System.Text;
using Actors;
using Actors.Monsters;
using Infrastructure;
using UnityEngine;
using World;

[RequireComponent(typeof(PlatformDetector))]
public class TestPlayer : MonoBehaviour, IPlayer
{
    // Front
    public int CurrentPlatform { get; private set; }

    public int HP => 100;
    public int MaxHP => 100;
    public bool IsAlive => true;

    // Property
    [SerializeField] private PlatformManager _platformManager;

    // Inspector
    [Header("Attack")]
    [Min(0)] public int AttackPower;
    [Header("Knockback")]
    public bool UseKnockback = true;
    public bool UseDefaultKnockbackForce = true;
    public float KnockbackForce = 0;
    [Header("State Disply")]
    [SerializeField, TextArea(3, 10)]
    private string _stateDisplay = string.Empty;
    private readonly StringBuilder _sb = new();

    // Internal
    private PlatformDetector _platformDetector;

#pragma warning disable CS0067
    public event Action<PlayerCondition> ConditionChanged;
    public event Action Destroyed;
#pragma warning restore


    // Content
    private void Awake()
    {
        if (!_platformManager)
            throw new InvalidOperationException(
                $"{typeof(TestPlayer).Name} 객체를 사용하려면 {nameof(_platformManager)} 컴포넌트가 할당되어 있어야 합니다.");

        _platformDetector = GetComponent<PlatformDetector>();
        _platformDetector.SetPlatformManager(_platformManager);
    }

    void IInjectable<PlatformManager>.Inject(PlatformManager platformManager)
    {
        _platformManager = platformManager;
        _platformDetector?.SetPlatformManager(platformManager);
    }

    private void Update()
    {
        if (_platformDetector.TryGetCurrentPlatformId(out var platformId))
            CurrentPlatform = platformId;
        else
            CurrentPlatform = -1;

        UpdateStateDisplay();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var receiver = collision.gameObject
            .GetComponentInChildren<MonsterDamageReceiver>();

        if (!receiver)
            return;

        if (UseKnockback)
        {
            var dir = (receiver.transform.position - transform.position).ToDirection();
            receiver.TakeDamage(AttackPower, dir, UseDefaultKnockbackForce ? null : KnockbackForce);
        }
        else
        {
            receiver.TakeDamage(AttackPower);
        }
    }


    private void UpdateStateDisplay()
    {
        _sb.Clear();
        _sb.AppendLine($"Current Platform: {(CurrentPlatform >= 0 ? CurrentPlatform : "null")}");

        _stateDisplay = _sb.ToString();
    }
}
