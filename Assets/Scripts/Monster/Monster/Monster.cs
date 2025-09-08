using System;
using System.Text;
using MonsterActions;
using MonsterBT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer), typeof(Animator), typeof(Rigidbody2D))]
[RequireComponent(typeof(PlatformDetector), typeof(MonsterHitted))]
public abstract partial class Monster : MonoBehaviour
{
    // Front
    public int HP
    {
        get => _hp;
        internal set => _hp = Mathf.Clamp(value, 0, MaxHP);
    }

    public Direction Direction
    {
        get => _direction;
        internal set
        {
            if (_direction == value) return;
            _direction = value;

            if (_direction == Direction.Left)
                transform.localScale = new Vector3(defaultIsRight ? -1f : 1f, 1f, 1f);
            else if (_direction == Direction.Right)
                transform.localScale = new Vector3(defaultIsRight ? 1f : -1f, 1f, 1f);
        }
    }

    public bool IsAlive { get; internal set; } = true;


    // Property 
    [Header("Stats Overrride")]
    [SerializeField] private bool overrideStats = true;
    [SerializeField] protected MonsterStats[] stats;

    [Header("Stats")]
    [SerializeField, Min(0)] private int _maxHP = 5;
    [SerializeField, Min(0)] private int _attackPower = 1;
    [SerializeField, Min(0)] private float _moveSpeed = 1;
    [SerializeField, Min(0)] private float _attackCooltime = 0.5f;
    [SerializeField, Min(0)] private float _invincibleDuration = 0.5f;

    [Header("Image Settings")]
    [SerializeField] protected bool randomizeStartDirection = true;
    [SerializeField] protected bool defaultIsRight;

    [Header("Bindings")]
    [SerializeField] internal PlatformManager platformManager;
    [SerializeField] internal SceneAssetsLibrary sceneAssetsLibrary;

    [Header("Animations")]
    [SerializeField, Min(0)] internal float damageFlashDuration = 0.1f;


    // Display
    [SerializeField, Header("Display"), TextArea(3, 15)]
    private string stateDisplay = string.Empty;
    private readonly StringBuilder sb = new();


    // Control
    protected bool UseStatsOverride => overrideStats && stats?.Length >= 1 && stats[0];

    public virtual int MaxHP => !UseStatsOverride ? _maxHP : stats[0].MaxHP;
    public virtual int AttackPower => !UseStatsOverride ? _attackPower : stats[0].AttackPower;
    public virtual float MoveSpeed => !UseStatsOverride ? _moveSpeed : stats[0].MoveSpeed;
    public virtual float AttackCooltime => !UseStatsOverride? _attackCooltime : stats[0].AttackCooltime;
    public virtual float InvincibleDuration => !UseStatsOverride ? _invincibleDuration : stats[0].InvincibleDuration;


    // Component
    internal Rigidbody2D Rigidbody { get; private set; }
    internal Animator Animator { get; private set; }
    internal SpriteRenderer SpriteRenderer { get; private set; }

    internal int BelongingPlatform { get; set; } = 1;
    internal PlatformDetector PlatformDetector { get; private set; }
    internal GameObject DetectedPlayer => playerDetector.CurrentPlayer;

    private MonsterHitted hitDetector;
    private MonsterPlayerDetector playerDetector;

    //internal 
    internal StandaloneHitAction StandaloneHitAction { get; private set; }
    internal StandaloneHitBrain StandaloneHitBrain { get; private set; }
    internal MonsterActionController ActionController { get; set; }
    internal MonsterBrain Brain { get; set; }


    // Internal
    private int _hp;
    private Direction _direction = Direction.Center;


    // Content
    protected virtual void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();

        if (!platformManager)
            throw new InvalidOperationException(Ctx(
                $"PlatformManager가 등록되어 있지 않기 때문에 몬스터를 시작할 수 없습니다."));

        if (!sceneAssetsLibrary)
            Debug.LogWarning(Ctx(
                $"이 몬스터는 SceneAssetsLibrary를 가지고 있지 않습니다. " +
                "관련 기능이 정상적으로 작동하지 않을 수 있습니다."));

        PlatformDetector = GetComponent<PlatformDetector>();
        PlatformDetector.SetPlatformManager(platformManager);

        playerDetector = GetComponentInChildren<MonsterPlayerDetector>();

        if (playerDetector)
            playerDetector.PlayerDetected += OnPlayerDetected;
        else
            Debug.LogWarning(Ctx(
                $"이 몬스터는 {nameof(MonsterPlayerDetector)}를 가지고 있지 않습니다. " +
                "플레이어 감지 기능이 정상적으로 작동하지 않을 수 있습니다."));

        hitDetector = GetComponent<MonsterHitted>();
        hitDetector.Damaged += OnDamaged;

        if (TryGetComponent<StandaloneHitAction>(out var standaloneHitAction))
        {
            StandaloneHitAction = standaloneHitAction;
            StandaloneHitAction.Initialize(this);

            StandaloneHitBrain = new StandaloneHitBrain(this);
        }

        _hp = MaxHP;
    }

    protected virtual void Start()
    {
        if (randomizeStartDirection)
        {
            Direction = UnityEngine.Random.Range(0, 2) == 0
                ? Direction.Left
                : Direction.Right;
        }
    }

    protected virtual void Update()
    {
        Brain?.Tick();

#if UNITY_EDITOR
        stateDisplay = GetDisplayContent();
#endif
    }

    protected void FixedUpdate()
    {
        ActionController?.Update();
    }

    protected virtual void OnPlayerDetected(GameObject player) { }
    protected virtual void OnDamaged(int damage) => HP -= damage;

    internal bool TryMove() => TryMove(Direction);
    internal bool TryMove(Direction direction)
    {
        if (Direction == Direction.Center)
            return true;

        if (direction == Direction.Left)
        {
            if (!PlatformDetector.CheckPlatform(Direction.Left, BelongingPlatform, out _))
                return false;

            Rigidbody.velocity = new Vector2
            {
                x = -MoveSpeed,
                y = Rigidbody.velocity.y,
            };

            return true;
        }

        if (direction == Direction.Right)
        {
            if (!PlatformDetector.CheckPlatform(Direction.Right, BelongingPlatform, out _))
                return false;

            Rigidbody.velocity = new Vector2
            {
                x = MoveSpeed,
                y = Rigidbody.velocity.y,
            };

            return true;
        }


        Debug.LogWarning(Ctx(
            $"현재 Direction 상태({Direction}')가 유효하지 않기 때문에 TryMove 메서드의 평가를 진행할 수 없습니다. false를 반환합니다."));

        return false;
    }

    internal void StopMoving() => Rigidbody.velocity = new Vector2
    {
        x = 0,
        y = Rigidbody.velocity.y,
    };



    #region Actions
    internal bool TryDoAction(
        MonsterAction monsterAction,
        out ActionResult reason,
        Action<ActionResult> callback = null,
        bool stopPreviousAction = true,
        bool allowRestart = false,
        float? playTime = null,
        float stayTimeAfterFinised = 0f)
        => ActionController.TryDoAction(monsterAction.ToString(), out reason, callback, stopPreviousAction, allowRestart, playTime, stayTimeAfterFinised);

    internal bool TryDoAction(
        string monsterAction,
        out ActionResult reason,
        Action<ActionResult> callback = null,
        bool stopPreviousAction = true,
        bool allowRestart = false,
        float? playTime = null,
        float stayTimeAfterFinised = 0f)
        => ActionController.TryDoAction(monsterAction, out reason, callback, stopPreviousAction, allowRestart, playTime, stayTimeAfterFinised);

    internal MonsterAction GetCurrentAction() => ActionController.GetCurrentAction();
    internal bool TryGetCurrentAction(out string name) => ActionController.TryGetCurrentAction(out name);

    internal void StopCurrentAction() => ActionController.StopCurrentAction();


    internal void Die()
    {
        Destroy(gameObject);
    }
    #endregion


    protected virtual void OnDestroy()
    {
        Brain?.Dispose();

        if (hitDetector)
            hitDetector.Damaged -= OnDamaged;

        if (playerDetector)
            playerDetector.PlayerDetected -= OnPlayerDetected;
    }


    internal string Ctx(string message) => $"[Monster '{name}'] {message}";

    protected virtual string GetDisplayContent()
    {
        sb.Clear();
        sb.AppendLine($"HP: {HP}");
        sb.AppendLine($"Direction: {Direction.ToString()}");
        sb.AppendLine($"Current Platform: {(BelongingPlatform >= 0 ? BelongingPlatform : "null")}");
        sb.AppendLine("----------------");
        sb.AppendLine($"Is Alive: {IsAlive}");
        if (StandaloneHitBrain != null) sb.AppendLine($"Is Damaging (SA): {StandaloneHitBrain.IsDamaging}");
        sb.AppendLine($"Is Committing: {Brain.Blackboard.Committing}");
        sb.AppendLine($"Current Action: {(TryGetCurrentAction(out var action) ? action : "None")}");

        if (Brain != null)
        {
            sb.AppendLine("----------------");
            sb.AppendLine(Brain.GetFullState());
        }

        return sb.ToString();
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Monster)), CanEditMultipleObjects]
    protected class MonsterEditor : Editor
    {
        protected string[] defaultHidingFields =
            new[] { "_maxHP", "_attackPower", "_moveSpeed", "_attackCooltime", "_invincibleDuration" };

        public override void OnInspectorGUI()
        {
            var target = (Monster)base.target;
            serializedObject.Update();

            if (target.UseStatsOverride)
                DrawPropertiesExcluding(serializedObject, GetHidingFields());
            else
                DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual string[] GetHidingFields() => defaultHidingFields;
    }
#endif
}
