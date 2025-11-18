using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;
using static Actors.Monsters.Brains.Engaged;

namespace Actors.Monsters.Stage3Bosses
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public class Belia : Monster<BeliaStats>, ITwinBoss
    {
        // Front
        public enum AttackType
        {
            Any,
            Slash,
            CurvedArea,
            Dash
        }

        [Header("Temp")]
        [SerializeField] private GameObject _player;
        [SerializeField] private bool _isAwaken = false;

        internal override GameObject DetectedPlayer => _player;


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
                            .AddChild(new Engaged(RangeType.Contact)
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack())
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
            public BeliaActionController(IMonsterInternal monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent("SlashAttack"));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }


        // Content
        protected override void Start()
        {
            base.Start();

            ActionController = new BeliaActionController(this);
            ActionController.Enter();

            Brain = new BeliaBrain(this);
        }

        protected override void Update()
        {
            Brain.Blackboard.Properties[ITwinBoss.IsAwaken] = _isAwaken;
            base.Update();  
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
    }
}
