using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters.Stage3Bosses
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class Belia : Monster<BeliaStats>, ITwinBoss
    {
        // Front
        public enum AttackMode
        {
            Any,
            Slash,
            CurvedArea,
            Dash
        }

        [Header("Belia")]
        [SerializeField] private GameObject _curveEffectPrefab;
        [SerializeField] private Vector2 _curveEffectWorldPosition;
        [SerializeField, Min(0)] private float _curveEffectLength = 1f;
        [Space]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [SerializeField] private float _dashStartTime = 0f;
        [SerializeField] private float _dashForce = 100f;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        public bool IsExhausted { get; set; } = false;
        internal override GameObject DetectedPlayer => _player?.gameObject;


        // Internal
        private class BeliaBrain : MonsterBrain
        {
            public BeliaBrain(IMonsterInternal owner) : base(owner)
            {
                Blackboard.Properties[ITwinBoss.IsAwaken] = false;

                AddChild(new Alive()
                    .AddChild(new Idle())
                    .AddChild(new Awaken()
                        .AddChild(new ValidPlatform() { HierarchyMode = HierarchyMode.Sequence }
                            .AddChild(new Engaged(Engaged.RangeType.Contact)
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new BeliaAttackBrain())
                        )
                        .AddChild(new NotValidPlatform())
                    )
                );
                AddChild(new Exhausted());
                AddChild(new Dead() { IsSelectable = false });
            }
        }

        private class BeliaActionController : MonsterActionController
        {
            public BeliaActionController(Belia belia) : base(belia)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction("SlashAttack")
                    .AddAnimationComponent());
                AddChild(new MonsterAction("CurvedAreaAttack")
                    .AddAnimationComponent(
                        "CurvedAreaAttackStart",
                        out var curvedAreaAttackEnter
                    )
                    .AddComponent(new BeliaCurvedAreaAttackAction(
                            belia._curveEffectPrefab,
                            belia._curveEffectWorldPosition,
                            belia._curveEffectLength),
                        out var curvedAreaAttackAction,
                        after: new(curvedAreaAttackEnter)
                    )
                    .AddAnimationComponent(
                        "CurvedAreaAttackEnd",
                        after: new(curvedAreaAttackAction))
                    );
                AddChild(new MonsterAction("DashAttack")
                    .AddAnimationComponent(interruptAllOnDeactivate: true)
                    .AddDelayComponent(belia._dashStartTime, out var delay)
                    .AddComponent(new BeliaDashAttackAction(belia._dashForce), after: new(delay)));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelayComponent()
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction("Exhausted")
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }

        private IPlayer _player;
        private bool _isAwaken = false;


        // Content
        public void Initialize(IPlayer player)
        {
            _player = player;
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new BeliaActionController(this);
            ActionController.Enter();

            Brain = new BeliaBrain(this);


            // ------- Debug -------
            if (_useTargetPlayer
                && _targetPlayer
                && _targetPlayer.TryGetComponent<IPlayer>(out var player))
                Initialize(player);

            if (_autoAwake)
                DoAwake();
        }

        protected override void Update()
        {
            Brain.Blackboard.Properties[ITwinBoss.IsAwaken] = _isAwaken;
            base.Update();  
        }

        public void DoAwake()
        {
            _isAwaken = true;
            Brain.Blackboard.Committing = true;
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            StandaloneHitBrain.TryTakeDamage(damageInfo);
        }

        public void Revive(float hpRate)
        {
            HP = Mathf.CeilToInt(hpRate * StatsInfo.MaxHP);
        }

        public void Die()
        {
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(nameof(Dead), null, EntryPolicy.Unconditional, RerunPolicy.EnsureRunningAndInjectInputs)
            });
        }

        protected override string GetDisplayContent()
        {
            var message = base.GetDisplayContent();

            message += "----------------";
            message += $"\nAwaken: {_isAwaken}";

            return message;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(Belia))]
        private class BeliaEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (Belia)base.target;

                if (!target._autoAwake && GUILayout.Button("Awake"))
                    target.DoAwake();
            }
        }
#endif
    }
}
