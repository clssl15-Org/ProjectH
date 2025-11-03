using System;
using System.Text;
using MonsterActions;
using MonsterBT;
using UnityEngine;
using static MonsterActions.MonsterAction;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
[RequireComponent(typeof(PlatformDetector))]
public abstract partial class Monster<TStats> : MonoBehaviour, IMonster where TStats : MonsterStats
{
    // Front
    public int HP
    {
        get => _hp;
        internal set => _hp = Mathf.Clamp(value, 0, StatsInfo.MaxHP);
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
    [Header("Stats")]
    [SerializeField] private bool _useDebugStatsInfo = false;
    [SerializeField] private TStats _statsInfo;
    [SerializeField] private TStats _debugStatsInfo;
    public TStats StatsInfo => _useDebugStatsInfo ? _debugStatsInfo : _statsInfo;

    [Header("Image Settings")]
    [SerializeField] protected bool RandomizeStartDirection = true;
    [SerializeField] internal bool DefaultIsRight;

    [Header("Bindings")]
    [SerializeField] internal SceneAssetsLibrary SceneAssetsLibrary;
    [SerializeField] internal PlatformManager PlatformManager;
    SceneAssetsLibrary IMonster.SceneAssetsLibrary => SceneAssetsLibrary;


    // Display
    [SerializeField, Header("Display"), TextArea(3, 15)]
    private string _stateDisplay = string.Empty;
    private readonly StringBuilder _sb = new();


    // Components
    internal SpriteRenderer SpriteRenderer { get; private set; }
    internal Collider2D Collider { get; private set; }
    internal Rigidbody2D Rigidbody { get; private set; }

    internal int BelongingPlatform { get; set; } = 1;
    internal PlatformDetector PlatformDetector { get; private set; }
    internal GameObject DetectedPlayer => _playerDetector.CurrentPlayer;

    private MonsterPlayerDetector _playerDetector;
    private MonsterDamageReceiver _monsterDamageReceiver;

    // Low-level Behavior Handlers
    private KnockbackHandler _knockbackHandler;
    internal MonsterAnimationPlayer AnimationPlayer { get; private set; }
    internal StandaloneHitAction StandaloneHitAction { get; private set; }
    internal StandaloneHitBrain StandaloneHitBrain { get; private set; }

    // High-level Behavior Handlers
    internal MonsterActionController ActionController { get; set; }
    internal MonsterBrain Brain { get; set; }

    #region Interfaces
    int IMonster.HP { get => HP; set => HP = value; }
    Direction IMonster.Direction { get => Direction; set => Direction = value; }
    bool IMonster.IsAlive { get => IsAlive; set => IsAlive = value; }
    MonsterStats IMonster.StatsInfo => StatsInfo;
    SpriteRenderer IMonster.SpriteRenderer => SpriteRenderer;
    Collider2D IMonster.Collider => Collider;
    Rigidbody2D IMonster.Rigidbody => Rigidbody;
    int IMonster.BelongingPlatform { get => BelongingPlatform; set => BelongingPlatform = value; }
    PlatformDetector IMonster.PlatformDetector => PlatformDetector;
    GameObject IMonster.DetectedPlayer => DetectedPlayer;
    MonsterAnimationPlayer IMonster.AnimationPlayer => AnimationPlayer;
    StandaloneHitAction IMonster.StandaloneHitAction => StandaloneHitAction;
    MonsterActionController IMonster.ActionController => ActionController;
    #endregion

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
            Debug.LogWarning(FormatLogMessage(
                $"이 몬스터는 {nameof(MonsterPlayerDetector)}을(를) 가지고 있지 않습니다. " +
                "플레이어 감지 기능이 정상적으로 작동하지 않을 수 있습니다."));


        _monsterDamageReceiver = GetComponentInChildren<MonsterDamageReceiver>(true);
        if (!_monsterDamageReceiver) throw new InvalidOperationException(FormatLogMessage(
            $"{nameof(_monsterDamageReceiver)}이(가) 존재하지 않기 때문에 몬스터를 시작할 수 없습니다."));

        _monsterDamageReceiver.Damaged += OnDamaged;

        if (TryGetComponent<StandaloneHitAction>(out var standaloneHitAction))
        {
            StandaloneHitAction = standaloneHitAction;
            StandaloneHitAction.Initialize(this);

            StandaloneHitBrain = new StandaloneHitBrain(this);
        }

        _knockbackHandler = new KnockbackHandler(Rigidbody);
        AnimationPlayer = new MonsterAnimationPlayer(GetComponent<Animator>());

        _direction = DefaultIsRight ? Direction.Right : Direction.Left;
        _hp = StatsInfo.MaxHP;

#if !UNITY_EDITOR
        _useDebugStatsInfo = false;
#endif
    }

    protected virtual void Start()
    {
        #region 필수 컴포넌트 설정
        if (!PlatformManager)
            throw new InvalidOperationException(FormatLogMessage(
                $"{nameof(PlatformManager)}이(가) 등록되어 있지 않기 때문에 몬스터를 시작할 수 없습니다."));

        if (!SceneAssetsLibrary)
            Debug.LogWarning(FormatLogMessage(
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
                x = -StatsInfo.MoveSpeed,
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
                x = StatsInfo.MoveSpeed,
                y = Rigidbody.velocity.y,
            };

            return true;
        }


        Debug.LogWarning(FormatLogMessage(
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

    #region Interfaces
    void IMonster.Knockback(Direction direction, float? knockbackForce) => Knockback(direction, knockbackForce);
    void IMonster.Die(bool succeed) => Die(succeed);
    #endregion
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

    #region Interfaces
    bool IMonster.TryDoAction(MonsterActionPlayInfo playInfo, out ActionResult reason, bool stopPreviousAction, bool allowRestart) =>
    TryDoAction(playInfo, out reason, stopPreviousAction, allowRestart);
    bool IMonster.TryMove() => TryMove();
    bool IMonster.TryMove(Direction direction) => TryMove(direction);
    void IMonster.StopMoving() => StopMoving();
    MonsterActionType IMonster.GetCurrentAction() => GetCurrentAction();
    bool IMonster.TryGetCurrentAction(out string name) => TryGetCurrentAction(out name);
    void IMonster.StopCurrentAction() => StopCurrentAction();
    #endregion
    #endregion


    protected virtual void OnDestroy()
    {
        Brain?.Dispose();

        if (_monsterDamageReceiver)
            _monsterDamageReceiver.Damaged -= OnDamaged;

        if (_playerDetector)
            _playerDetector.PlayerDetected -= OnPlayerDetected;
    }


    public string FormatLogMessage(string message) => $"[{name}] {message}";

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
}
