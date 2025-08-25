using System;
using System.Text;
using UnityEngine;
using MonsterBT;
using MonsterActions;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
[RequireComponent(typeof(PlatformDetector), typeof(MonsterHitted))]
public abstract partial class Monster : MonoBehaviour
{
    // Front
    public int HP
    {
        get => _hp;
        internal set => _hp = Mathf.Clamp(value, 0, maxHp);
    }

    public int AttackPower => attackPower;
    public int MoveSpeed => moveSpeed;

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

    // Property 
    [Header("Parameters")]
    [SerializeField, Min(0)] private int maxHp;
    [SerializeField, Min(0)] private int attackPower;
    [SerializeField, Min(0)] private int moveSpeed;
    [SerializeField] protected bool defaultIsRight;

    [Header("Bindings")]
    [SerializeField] protected PlatformManager platformManager;

    // Display
    [SerializeField, Header("Display"), TextArea(3, 15)]
    private string stateDisplay = string.Empty;
    private readonly StringBuilder sb = new();

    // Front
    public bool IsAlive { get; internal set; } = false;

    // Internal
    internal Rigidbody2D Rigidbody { get; private set; }
    internal Animator Animator { get; private set; }

    internal int BelongingPlatform { get; set; } = 1;
    internal PlatformDetector PlatformDetector { get; private set; }
    internal GameObject DetectedPlayer => playerDetector.CurrentPlayer;

    private int _hp;
    private Direction _direction = Direction.Center;

    private MonsterHitted hitDetector;
    private MonsterPlayerDetector playerDetector;

    internal MonsterActionController ActionController { get; set; }
    internal MonsterBrain Brain { get; set; }


    // Content
    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();

        if (!platformManager)
            throw new InvalidOperationException(Ctx(
                $"PlatformManager가 등록되어 있지 않기 때문에 몬스터를 시작할 수 없습니다."));

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

        _hp = maxHp;
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

        throw new InvalidOperationException(Ctx(
            $"현재 Direction 상태({Direction}')가 유효하지 않기 때문에 TryMove 메서드를 수행할 수 없습니다."));
    }

    #region Actions
    internal bool TryDoAction(
        MonsterAction monsterAction,
        out ActionResult reason,
        Action<ActionResult> callback = null,
        bool stopPreviousAction = true,
        bool allowRestart = false,
        float? playTime = null)
        => ActionController.TryDoAction(monsterAction, out reason, callback, stopPreviousAction, allowRestart, playTime);

    internal MonsterAction GetCurrentAction() => ActionController.GetCurrentAction();
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


    internal string Ctx(string message) => $"[Monster '{GetType().Name}'] {message}";

    protected virtual string GetDisplayContent()
    {
        sb.Clear();
        sb.AppendLine($"HP: {HP}");
        sb.AppendLine($"Direction: {Direction.ToString()}");
        sb.AppendLine($"Current Platform: {(BelongingPlatform >= 0 ? BelongingPlatform : "null")}");
        sb.AppendLine($"Current Action: {GetCurrentAction().ToString()}");

        if (Brain != null)
        {
            sb.AppendLine("----------------");
            sb.AppendLine(Brain.GetFullState());
        }

        return sb.ToString();
    }
}
