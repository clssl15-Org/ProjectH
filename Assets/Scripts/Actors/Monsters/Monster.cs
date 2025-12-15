using System;
using System.Text;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using UnityEngine;
using World;

namespace Actors.Monsters
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteSizeHandler), typeof(Animator))]
    [RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlatformDetector))]
    public abstract partial class Monster<TStats>
        : MonoBehaviour, IMonsterInternal, IMonster where TStats : MonsterStats
    {
        // Front
        public int HP
        {
            get => _hp;
            internal set => _hp = value;
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

        public virtual bool IgnorePlayerInteraction
        {
            get => _ignorePlayerInternaction;
            set
            {
                _ignorePlayerInternaction = value;

                var layer = value
                    ? LayerMask.GetMask("Player")
                    : default;

                Collider.excludeLayers = layer;
                Rigidbody.excludeLayers = layer;
            }
        }

        public int MaxHP => StatsInfo.MaxHP;
        public bool IsAlive { get; internal set; } = true;

        public event Action<IMonsterConditionData> ConditionChanged;
        public event Action Destroyed;

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
        [SerializeField] internal GameAssetLibrary GameAssetsLibrary;
        [SerializeField] internal Configuration Configuration;
        [SerializeField] internal PlatformManager PlatformManager;


        // Display
        [SerializeField, Header("Display"), TextArea(3, 15)]
        private string _stateDisplay = string.Empty;
        private readonly StringBuilder _sb = new();


        // Components
        internal SpriteRenderer SpriteRenderer { get; private set; }
        private SpriteSizeHandler _spriteSizeHandler;
        internal Collider2D Collider { get; private set; }
        internal Rigidbody2D Rigidbody { get; private set; }

        public int CurrentPlatform { get; set; } = 1;
        internal virtual PlatformDetector PlatformDetector { get; private set; }
        internal virtual GameObject DetectedPlayer => _playerDetector.CurrentPlayer;

        private MonsterPlayerDetector _playerDetector;
        protected MonsterDamageReceiver DamageReceiver { get; private set; }

        // Low-level Behavior Handlers
        private KnockbackHandler _knockbackHandler;
        internal MonsterAnimationPlayer AnimationPlayer { get; private set; }
        internal StandaloneHitAction StandaloneHitAction { get; private set; }
        internal StandaloneHitBrain StandaloneHitBrain { get; private set; }

        // High-level Behavior Handlers
        internal MonsterActionController ActionController { get; set; }
        internal MonsterBrain Brain { get; set; }

        #region Interfaces
        int IMonsterInternal.HP { get => HP; set => HP = value; }
        Direction IMonsterInternal.Direction { get => Direction; set => Direction = value; }
        bool IMonsterInternal.IsAlive { get => IsAlive; set => IsAlive = value; }
        MonsterStats IMonsterInternal.StatsInfo => StatsInfo;
        SpriteRenderer IMonsterInternal.SpriteRenderer => SpriteRenderer;
        Collider2D IMonsterInternal.Collider => Collider;
        Rigidbody2D IMonsterInternal.Rigidbody => Rigidbody;
        int IMonsterInternal.CurrentPlatform { get => CurrentPlatform; set => CurrentPlatform = value; }
        GameAssetLibrary IMonsterInternal.GameAssetsLibrary => GameAssetsLibrary;
        Configuration IMonsterInternal.Configuration => Configuration;
        PlatformManager IMonsterInternal.PlatformManager => PlatformManager;
        PlatformDetector IMonsterInternal.PlatformDetector => PlatformDetector;
        GameObject IMonsterInternal.DetectedPlayer => DetectedPlayer;
        MonsterAnimationPlayer IMonsterInternal.AnimationPlayer => AnimationPlayer;
        StandaloneHitAction IMonsterInternal.StandaloneHitAction => StandaloneHitAction;
        MonsterActionController IMonsterInternal.ActionController => ActionController;
        #endregion

        // Internal 
        private int _hp;
        private Direction _direction;
        bool _ignorePlayerInternaction = false;


        // Content
        #region Injections
        /// <summary>
        /// 외부에서 몬스터를 직접 생성할 경우 이 메서드를 호출하여 필수 컴포넌트를 할당하세요.
        /// </summary>
        public void Initialize(
            GameAssetLibrary gameAssetsLibrary,
            Configuration configuration,
            PlatformManager platformManager)
        {
            Inject(gameAssetsLibrary);
            Inject(configuration);
            Inject(platformManager);
        }

        void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetsLibrary) => Inject(gameAssetsLibrary);
        void IInjectable<Configuration>.Inject(Configuration configuration) => Inject(configuration);
        void IInjectable<PlatformManager>.Inject(PlatformManager platformManager) => Inject(platformManager);

        private void Inject(GameAssetLibrary gameAssetsLibrary)
        {
            GameAssetsLibrary = gameAssetsLibrary;
        }

        private void Inject(Configuration configuration)
        {
            Configuration = configuration;

            if (TryGetComponent<SpriteSizeHandler>(out var sizeHandler))
                sizeHandler.Initialize(configuration, true);
        }

        private void Inject(PlatformManager platformManager)
        {
            PlatformManager = platformManager;
        }
        #endregion

        protected virtual void Awake()
        {
            Collider = GetComponent<Collider2D>();
            Rigidbody = GetComponent<Rigidbody2D>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            _spriteSizeHandler = GetComponent<SpriteSizeHandler>();

            _playerDetector = GetComponentInChildren<MonsterPlayerDetector>();
            if (_playerDetector) _playerDetector.PlayerDetected += OnPlayerDetected;


            DamageReceiver = GetComponentInChildren<MonsterDamageReceiver>(true);
            if (!DamageReceiver) throw new InvalidOperationException(FormatLogMessage(
                $"{nameof(DamageReceiver)}이(가) 존재하지 않기 때문에 몬스터를 시작할 수 없습니다."));

            DamageReceiver.Damaged += OnDamaged;

            if (TryGetComponent<StandaloneHitAction>(out var standaloneHitAction))
            {
                StandaloneHitAction = standaloneHitAction;
                StandaloneHitAction.Initialize(this);

                StandaloneHitBrain = new StandaloneHitBrain(this);
            }

            _knockbackHandler = new KnockbackHandler(Rigidbody);
            AnimationPlayer = new MonsterAnimationPlayer(
                GetComponent<Animator>(),
                () => _spriteSizeHandler?.RequestApplyScaleFactor());

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

            if (!GameAssetsLibrary)
                Debug.LogWarning(FormatLogMessage(
                    $"이 몬스터는 {nameof(GameAssetsLibrary)}을(를) 가지고 있지 않습니다. " +
                    "관련 기능이 정상적으로 작동하지 않을 수 있습니다."));

            var _platformDetector = GetComponent<PlatformDetector>();
            PlatformDetector = _platformDetector;
            _platformDetector.SetPlatformManager(PlatformManager);
            #endregion

            _spriteSizeHandler.RequestApplyScaleFactor();

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

        protected virtual void FixedUpdate()
        {
            ActionController?.Update();
        }

        protected virtual void OnPlayerDetected(GameObject player) { }
        protected virtual void OnDamaged(DamageInfo damageInfo)
        {
            HP -= damageInfo.Damage;

            var notification = new MonsterConditionData(MonsterCondition.Damage);
            ConditionChanged?.Invoke(notification);
            notification.Complete();
        }


        #region Low-level Actions
        internal bool TryMove() => TryMove(Direction);
        internal bool TryMove(Direction direction)
        {
            if (Direction == Direction.Center)
                return true;

            if (direction == Direction.Left)
            {
                if (!PlatformDetector.CheckPlatform(Direction.Left, CurrentPlatform, out _))
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
                if (!PlatformDetector.CheckPlatform(Direction.Right, CurrentPlatform, out _))
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

        internal virtual void Died()
        {
            var notification = new MonsterConditionData(MonsterCondition.Die);
            ConditionChanged?.Invoke(notification);
            notification.Complete();

            Destroyed?.Invoke();

            ConditionChanged = null;
            Destroyed = null;

            Destroy(gameObject);
        }

        #region Interfaces
        void IMonsterInternal.Knockback(Direction direction, float? knockbackForce) => Knockback(direction, knockbackForce);
        void IMonsterInternal.NotifyCondition(IMonsterConditionData data) => ConditionChanged?.Invoke(data);
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
        bool IMonsterInternal.TryDoAction(MonsterActionPlayInfo playInfo, out ActionResult reason, bool stopPreviousAction, bool allowRestart) =>
        TryDoAction(playInfo, out reason, stopPreviousAction, allowRestart);
        bool IMonsterInternal.TryMove() => TryMove();
        bool IMonsterInternal.TryMove(Direction direction) => TryMove(direction);
        void IMonsterInternal.StopMoving() => StopMoving();
        MonsterActionType IMonsterInternal.GetCurrentAction() => GetCurrentAction();
        bool IMonsterInternal.TryGetCurrentAction(out string name) => TryGetCurrentAction(out name);
        void IMonsterInternal.StopCurrentAction() => StopCurrentAction();
        #endregion
        #endregion


        public void Destroy() => Destroy(gameObject);
        protected virtual void OnDestroy()
        {
            Destroyed?.Invoke();
            Brain?.Dispose();

            ConditionChanged = null;
            Destroyed = null;
        }


        public string FormatLogMessage(string message) => $"[{name}] {message}";

        protected virtual string GetDisplayContent()
        {
            _sb.Clear();
            _sb.AppendLine($"HP: {HP}");
            _sb.AppendLine($"Direction: {Direction.ToString()}");
            _sb.AppendLine($"Current Platform: {(CurrentPlatform >= 0 ? CurrentPlatform : "null")}");
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
}
