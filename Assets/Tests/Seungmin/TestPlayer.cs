using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Actors;
using Actors.Monsters;
using Infrastructure;
using Rules;
using UnityEngine;
using World;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlatformDetector), typeof(TriggerContactHandler))]
public class TestPlayer : MonoBehaviour, IPlayer, IDamageable
{
    // Front
    public int CurrentPlatform { get; private set; }

    public int HP
    {
        get => _hp;
        private set => _hp = Mathf.Clamp(value, 0, MaxHP);
    } int _hp;
    [field: SerializeField] public int MaxHP { get; set; } = 10;
    [field: SerializeField] public bool IsAlive { get; set; } = true;

<<<<<<< Updated upstream
=======
    [field: SerializeField] public int SelectedSkillIndex { get; set; } = 0;

    public float UltimateGauge => 0;

>>>>>>> Stashed changes
    public event Action<PlayerCondition> ConditionChanged;
    public event Action Destroyed;

    // Property
    [SerializeField] private PlatformManager _platformManager;
    [SerializeField] private TriggerContactHandler _contactHandler;

    // Inspector
    [Header("Input")]
    public bool StandaloneInput = true;
    [Header("Move")]
    [Min(0)] public float MoveSpeed = 1f;
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
    private SpriteRenderer _renderer;
    private PlatformDetector _platformDetector;

    private IDisposable _damageTimer;


    // Content
    private void Awake()
    {
        if (!_platformManager)
            throw new InvalidOperationException(
                $"[{nameof(TestPlayer)}] {nameof(_platformManager)} 컴포넌트가 유효하지 않습니다.");

        _renderer = GetComponent<SpriteRenderer>();

        _platformDetector = GetComponent<PlatformDetector>();
        _platformDetector.SetPlatformManager(_platformManager);

        _contactHandler = GetComponent<TriggerContactHandler>();
        _contactHandler.TargetTags = new[] { "Monster" };

        _hp = MaxHP;
    }

    void IInjectable<PlatformManager>.Inject(PlatformManager platformManager)
    {
        _platformManager = platformManager;
        _platformDetector?.SetPlatformManager(platformManager);
    }

    private void Update()
    {
        #region Move
        var speed = MoveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.W))
            transform.position += speed * Vector3.up;
        if (Input.GetKey(KeyCode.A))
            transform.position += speed * Vector3.left;
        if (Input.GetKey(KeyCode.S))
            transform.position += speed * Vector3.down;
        if (Input.GetKey(KeyCode.D))
            transform.position += speed * Vector3.right;
        #endregion

        if (_platformDetector.TryGetCurrentPlatformId(out var platformId))
            CurrentPlatform = platformId;
        else
            CurrentPlatform = -1;

        if (StandaloneInput && Input.GetKeyDown(KeyCode.Space))
            DefaultAttack();

        UpdateStateDisplay();
    }

    public void DefaultAttack()
    { 
        if (!_contactHandler || _contactHandler.Collisions.Count == 0)
            return;

        HashSet<MonsterDamageReceiver> interacted = null;

        foreach (var contact in _contactHandler.Collisions)
        {
            var receiver = contact.gameObject
                .GetComponentInChildren<MonsterDamageReceiver>();

            if (!receiver
                || !receiver.Interactable
                || (interacted?.Contains(receiver) ?? false))
                continue;

            if (UseKnockback)
            {
                var dir = (receiver.transform.position - transform.position).ToDirection();
                receiver.TakeDamage(AttackPower, dir, UseDefaultKnockbackForce ? null : KnockbackForce);
            }
            else
            {
                receiver.TakeDamage(AttackPower);
            }

            interacted ??= new();
            interacted.Add(receiver);
        }
    }

    public void TakeDamage(int damage) => TakeDamage(damage, Direction.Center);
    public void TakeDamage(int damage, Direction direction, float? knockbackForce = null)
    {
        print("Damaged: " + damage);
        _damageTimer?.Dispose();

        HP -= damage;
        if (HP <= 0)
        {
            if (gameObject)
                Destroy(gameObject);

            return;
        }

        _renderer.material.color = Color.red;
        _damageTimer = new Timer(0.1f, succeeded =>
        {
            if (succeeded)
                _renderer.material.color = Color.white;
        });

        ConditionChanged?.Invoke(PlayerCondition.Damage);
    }

    private void UpdateStateDisplay()
    {
        _sb.Clear();
        _sb.AppendLine($"Current Platform: {(CurrentPlatform >= 0 ? CurrentPlatform : "null")}");

        _stateDisplay = _sb.ToString();
    }

    private void OnDestroy()
    {
        _damageTimer?.Dispose();
        _damageTimer = null;

        IsAlive = false;
        Destroyed?.Invoke();
    }
}
