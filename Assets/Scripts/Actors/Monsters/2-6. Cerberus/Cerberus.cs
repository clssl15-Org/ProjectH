using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class Cerberus : Monster<CerberusStats>
    {
        // Front
        public enum AttackMode
        {
            Any,
            Bite,
            Drop,
            Ambush,
        }

        [Header("Cerberus")]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [SerializeField] private FallingStoneManager _fallingStoneManager;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        internal override GameObject DetectedPlayer => _player?.gameObject;
        private const string IsAwake = nameof(IsAwake);


        // Internal
        private class CerberusBrain : MonsterBrain
        {
            public CerberusBrain(IMonsterInternal owner) : base(owner)
            {
                Blackboard.Properties[IsAwake] = false;

                AddChild(new Alive()
                    .AddChild(new Idle(IsAwake))
                    .AddChild(new Awaken(IsAwake)
                        {
                            HierarchyMode = HierarchyMode.Sequence,
                            LoopType = LoopType.None,
                        }
                        .AddChild(new Attack("DropAttack"))
                        .AddChild(new CerberusAttackPhaseBrain()
                            .AddChild(new CerberusAttackBrain())
                            .AddChild(new Await(
                                MonsterActionType.Idle,
                                Owner.StatsInfo.AttackCooltime
                            ))
                        )
                    )
                );
                AddChild(new Dead() { DestroyOwnerOnCompleted = false });
            }
        }

        private class CerberusActionController : MonsterActionController
        {
            public CerberusActionController(Cerberus cerberus) : base(cerberus)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction("BiteAttack")
                    .AddAnimationComponent());
                AddChild(new MonsterAction("DropAttack")
                    .AddAnimationComponent("Roar")
                    .AddDelayComponent(1f, out var roar_delay)
                    .AddComponent(new Do(false)
                        .AssignTo(out var roar_doFall)
                        .OnOpening(() => cerberus._fallingStoneManager.DoFall(succeed =>
                            roar_doFall.Interrupt(succeed ? InterruptType.Completed : InterruptType.Error))
                        ),
                        after: new(roar_delay)
                    )
                    .AddDelayComponent(0.5f, after: new(roar_doFall))
                );
                AddChild(new MonsterAction("AmbushAttack")
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }

        private IPlayer _player;


        // Content
        public void InitializePlayer(IPlayer player)
        {
            _player = player;
        }

        protected override void Awake()
        {
            if (!_fallingStoneManager)
                throw new System.InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_fallingStoneManager)}' 컴포넌트를 가지고 있어야 합니다.");

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new CerberusActionController(this);
            ActionController.Enter();

            StandaloneHitBrain.DoKnockback = false;
            Brain = new CerberusBrain(this);


            // ------- Debug -------
            if (_useTargetPlayer
                && _targetPlayer
                && _targetPlayer.TryGetComponent<IPlayer>(out var player))
                InitializePlayer(player);

            if (_autoAwake)
                DoAwake();
        }

        public void DoAwake()
        {
            Brain.Blackboard.Properties[IsAwake] = true;
            Brain.Blackboard.Committing = true;
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            StandaloneHitBrain.TryTakeDamage(damageInfo);
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
            message += $"\nAwaken: {Brain.Blackboard.Properties[IsAwake]}";

            return message;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(Cerberus))]
        private class CerberusEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (Cerberus)base.target;

                if (!target._autoAwake && GUILayout.Button("Awake"))
                    target.DoAwake();
            }
        }
#endif
    }
}
