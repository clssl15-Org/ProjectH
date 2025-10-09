using System;
using System.Text;
using MonsterActions;
using MonsterBT;
using UnityEngine;
using static MonsterActions.MonsterAction;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
[RequireComponent(typeof(PlatformDetector))]
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
                transform.localScale = new Vector3(DefaultIsRight ? -1f : 1f, 1f, 1f);
            else if (_direction == Direction.Right)
                transform.localScale = new Vector3(DefaultIsRight ? 1f : -1f, 1f, 1f);
        }
    }

    public bool IsAlive { get; internal set; } = true;
    public event Action<bool> Died;


    // Property 
    [Header("Stats Overrride")]
    [SerializeField] private bool _overrideStats = false;
    [SerializeField] protected MonsterStats[] Stats;

    [Header("Stats")]
    [SerializeField, Min(0)] private int _maxHP = 3;
    [SerializeField, Min(0)] private int _attackPower = 1;
    [SerializeField, Min(0)] private float _moveSpeed = 1;
    [SerializeField, Min(0)] private float _attackCooltime = 0.5f;
    [SerializeField, Min(0)] private float _invincibleDuration = 0.5f;

    [Header("Image Settings")]
    [SerializeField] protected bool RandomizeStartDirection = true;
    [SerializeField] protected bool DefaultIsRight;

    [Header("Bindings")]
    [SerializeField] internal PlatformManager PlatformManager;
    [SerializeField] internal SceneAssetsLibrary SceneAssetsLibrary;

    [Header("Animations")]
    [SerializeField, Min(0)] internal float DamageFlashDuration = 0.1f;


    // Display
    [SerializeField, Header("Display"), TextArea(3, 15)]
    private string _stateDisplay = string.Empty;
    private readonly StringBuilder _sb = new();


    // Control
    protected bool UseStatsOverride => _overrideStats && Stats?.Length >= 1 && Stats[0];

    public virtual int MaxHP => !UseStatsOverride ? _maxHP : Stats[0].MaxHP;
    public virtual int AttackPower => !UseStatsOverride ? _attackPower : Stats[0].AttackPower;
    public virtual float MoveSpeed => !UseStatsOverride ? _moveSpeed : Stats[0].MoveSpeed;
    public virtual float AttackCooltime => !UseStatsOverride? _attackCooltime : Stats[0].AttackCooltime;
    public virtual float InvincibleDuration => !UseStatsOverride ? _invincibleDuration : Stats[0].InvincibleDuration;


    // Components
    internal SpriteRenderer SpriteRenderer { get; private set; }
    internal Collider2D Collider { get; private set; }
    internal Rigidbody2D Rigidbody { get; private set; }

    internal int BelongingPlatform { get; set; } = 1;
    internal PlatformDetector PlatformDetector { get; private set; }
    internal GameObject DetectedPlayer => _playerDetector.CurrentPlayer;

    private MonsterPlayerDetector _playerDetector;
    private MonsterHitted _hitDetector;

    // Low-level Behavior Handlers
    private KnockbackHandler _knockbackHandler;
    internal MonsterAnimationPlayer AnimationPlayer { get; private set; }
    internal StandaloneHitAction StandaloneHitAction { get; private set; }
    internal StandaloneHitBrain StandaloneHitBrain { get; private set; }

    // High-level Behavior Handlers
    internal MonsterActionController ActionController { get; set; }
    internal MonsterBrain Brain { get; set; }

    // Internal 
    private int _hp;
    private Direction _direction;


    // Content
    /// <summary>
    /// 외부에서 몬스터를 직접 생성할 경우 이 메서드를 호출하여 필수 컴포넌트를 할당하세요.
    /// </summary>
    public void Initialize(PlatformManager platformManager, SceneAssetsLibrary sceneAssetsLibrary)
    {
        PlatformManager = platformManager;
        SceneAssetsLibrary = sceneAssetsLibrary;
    }

    protected virtual void Awake()
    {
        Collider = GetComponent<Collider2D>();
        Rigidbody = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();

        _playerDetector = GetComponentInChildren<MonsterPlayerDetector>();

        if (_playerDetector)
            _playerDetector.PlayerDetected += OnPlayerDetected;
        else
            Debug.LogWarning(Ctx(
                $"이 몬스터는 {nameof(MonsterPlayerDetector)}을(를) 가지고 있지 않습니다. " +
                "플레이어 감지 기능이 정상적으로 작동하지 않을 수 있습니다."));


        _hitDetector = GetComponentInChildren<MonsterHitted>(true);
        if (!_hitDetector) throw new InvalidOperationException(Ctx(
            $"{nameof(_hitDetector)}이(가) 존재하지 않기 때문에 몬스터를 시작할 수 없습니다."));

        _hitDetector.Damaged += OnDamaged;

        if (TryGetComponent<StandaloneHitAction>(out var standaloneHitAction))
        {
            StandaloneHitAction = standaloneHitAction;
            StandaloneHitAction.Initialize(this);

            StandaloneHitBrain = new StandaloneHitBrain(this);
        }

        _knockbackHandler = new KnockbackHandler(Rigidbody);
        AnimationPlayer = new MonsterAnimationPlayer(GetComponent<Animator>());

        _direction = DefaultIsRight ? Direction.Right : Direction.Left;
        _hp = MaxHP;
    }

    protected virtual void Start()
    {
        #region 필수 컴포넌트 설정
        if (!PlatformManager)
            throw new InvalidOperationException(Ctx(
                $"{nameof(PlatformManager)}이(가) 등록되어 있지 않기 때문에 몬스터를 시작할 수 없습니다."));

        if (!SceneAssetsLibrary)
            Debug.LogWarning(Ctx(
                $"이 몬스터는 {nameof(SceneAssetsLibrary)}을(를) 가지고 있지 않습니다. " +
                "관련 기능이 정상적으로 작동하지 않을 수 있습니다."));

        PlatformDetector = GetComponent<PlatformDetector>();
        PlatformDetector.SetPlatformManager(PlatformManager);
        #endregion


        if (RandomizeStartDirection)
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
        _stateDisplay = GetDisplayContent();
#endif
    }

    protected void FixedUpdate()
    {
        ActionController?.Update();
    }

    protected virtual void OnPlayerDetected(GameObject player) { }
    protected virtual void OnDamaged(DamageInfo damageInfo) => HP -= damageInfo.Damage;

    #region Low-level Actions
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
            $"현재 입력된 {nameof(direction)}({Direction})이(가) 유효하지 않기 때문에 TryMove 메서드의 평가를 진행할 수 없습니다. false를 반환합니다."));

        return false;
    }

    internal void StopMoving() => Rigidbody.velocity = new Vector2
    {
        x = 0,
        y = Rigidbody.velocity.y,
    };

    internal void Knockback(Direction direction, float? knockbackForce = null) =>
        _knockbackHandler.Knockback(direction, knockbackForce);

    internal void Die(bool succeed)
    {
        Died?.Invoke(succeed);
        Destroy(gameObject);
    }
    #endregion


    #region High-level Actions
    internal bool TryDoAction(
        MonsterActionPlayInfo playInfo,
        out ActionResult reason,
        bool stopPreviousAction = true,
        bool allowRestart = false)
        => ActionController.TryDoAction(playInfo, out reason, stopPreviousAction, allowRestart);

    internal MonsterActionType GetCurrentAction() => ActionController.GetCurrentAction();
    internal bool TryGetCurrentAction(out string name) => ActionController.TryGetCurrentAction(out name);

    internal void StopCurrentAction() => ActionController.StopCurrentAction();
    #endregion


    protected virtual void OnDestroy()
    {
        Brain?.Dispose();

        if (_hitDetector)
            _hitDetector.Damaged -= OnDamaged;

        if (_playerDetector)
            _playerDetector.PlayerDetected -= OnPlayerDetected;
    }


    internal string Ctx(string message) => $"[{name}] {message}";

    protected virtual string GetDisplayContent()
    {
        _sb.Clear();
        _sb.AppendLine($"HP: {HP}");
        _sb.AppendLine($"Direction: {Direction.ToString()}");
        _sb.AppendLine($"Current Platform: {(BelongingPlatform >= 0 ? BelongingPlatform : "null")}");
        _sb.AppendLine("----------------");
        _sb.AppendLine($"Is Alive: {IsAlive}");
        if (StandaloneHitBrain != null) _sb.AppendLine($"Is Damaging (SA): {StandaloneHitBrain.IsDamaging}");
        _sb.AppendLine($"Is Committing: {Brain.Blackboard.Committing}");
        _sb.AppendLine($"Current Action: {(TryGetCurrentAction(out var action) ? action : "None")}");

        if (Brain != null)
        {
            _sb.AppendLine("----------------");
            _sb.AppendLine(Brain.GetFullState());
        }

        return _sb.ToString();
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Monster)), CanEditMultipleObjects]
    protected class MonsterEditor : Editor
    {
        private string[] _defaultHidingFields =
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

        protected virtual string[] GetHidingFields() => _defaultHidingFields;
    }
#endif
}
