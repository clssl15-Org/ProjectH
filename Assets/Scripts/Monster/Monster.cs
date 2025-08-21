using System;
using System.Text;
using UnityEngine;
using Infrastructure;
using MonsterBT;

[RequireComponent(typeof(Rigidbody2D), typeof(PlatformDetector), typeof(MonsterHitted))]
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
    [SerializeField] private bool defaultIsRight;

    [Header("Bindings")]
    [SerializeField] private PlatformManager platformManager;

    internal bool HasAnimator => animatorCallbackNotifier;
    /// <summary>
    /// Monster가 애니메이션을 가지고 있을 경우, 애니메이션의 시작과 끝 지점에 콜백을 등록할 수 있습니다.
    /// <para>HasAnimator 프로퍼티가 true일 때만 이 프로퍼티를 사용할 수 있습니다.</para>
    /// </summary>
    internal event AnimatorCallbackDelegate AnimatorCallback
    {
        add
        {
            if (!HasAnimator)
                throw new InvalidOperationException(Ctx(
                    "애니메이터가 없기 때문에 AnimatorCallback 이벤트를 사용할 수 없습니다."));

            _animationCallback += value;
        }
        remove
        {
            if (!HasAnimator)
                throw new InvalidOperationException(Ctx(
                    "애니메이터가 없기 때문에 AnimatorCallback 이벤트를 사용할 수 없습니다."));

            _animationCallback -= value;
        }
    }

    // Display
    [SerializeField, Header("Display"), TextArea(3, 10)]
    private string stateDisplay = string.Empty;
    private readonly StringBuilder sb = new();

    // Internal
    internal Rigidbody2D Rigidbody { get; private set; }
    internal int BelongingPlatform { get; set; } = 1;
    internal PlatformDetector PlatformDetector { get; private set; }

    internal GameObject DetectedPlayer => playerDetector.CurrentPlayer;

    private Animator animator;
    private AnimatorCallbackDelegate _animationCallback;

    private int _hp;
    private Direction _direction = Direction.Center;

    private AnimatorCallbackNotifier animatorCallbackNotifier;
    private MonsterHitted hitDetector;
    private MonsterPlayerDetector playerDetector;

    protected MonsterBrain Brain { get; set; }

    // Internal State
    protected bool Attacking { get; set; } = false;
    protected bool Damaging { get; set; } = false;
    protected bool Alive { get; set; } = true;


    // Content
    protected virtual void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();

        if (TryGetComponent(out animator))
        {
            animatorCallbackNotifier = animator.GetBehaviour<AnimatorCallbackNotifier>();
            if (animatorCallbackNotifier) animatorCallbackNotifier.Callback += OnAnimationChanged;
        }

        if (!platformManager)
        {
            throw new InvalidOperationException(Ctx(
                $"PlatformManager가 등록되어 있지 않기 때문에 몬스터를 시작할 수 없습니다."));
        }

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

    private void OnAnimationChanged(AnimatorStateInfo stateInfo, bool isEnter) => _animationCallback?.Invoke(stateInfo, isEnter);


    public void Die()
    {
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        Brain?.Dispose();

        if (animatorCallbackNotifier)
            animatorCallbackNotifier.Callback -= OnAnimationChanged;

        if (hitDetector)
            hitDetector.Damaged -= OnDamaged;

        if (playerDetector)
            playerDetector.PlayerDetected -= OnPlayerDetected;
    }


    protected string Ctx(string message) => $"[Monster '{GetType().Name}'] {message}";

    protected virtual string GetDisplayContent()
    {
        sb.Clear();
        sb.AppendLine($"HP: {HP}");
        sb.AppendLine($"Direction: {Direction.ToString()}");
        sb.AppendLine($"Current Platform: {(BelongingPlatform >= 0 ? BelongingPlatform : "null")}");
        sb.AppendLine("----------------");
        sb.AppendLine($"Alive: {Alive}");
        sb.AppendLine($"Attacking: {Attacking}");
        sb.AppendLine($"GettingDamage: {Damaging}");

        if (Brain != null)
        {
            sb.AppendLine("----------------");
            //sb.AppendLine(Brain.GetFullState());
        }

        return sb.ToString();
    }
}
