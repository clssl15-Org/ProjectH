using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class Ghost : Monster<GhostStats>
    {
        // Front
        public override bool IgnorePlayerInteraction
        {
            get => base.IgnorePlayerInteraction;
            set
            {
                base.IgnorePlayerInteraction = value;

                DamageReceiver.GetComponent<Collider2D>().excludeLayers = value
                    ? LayerMask.GetMask("Player")
                    : default;
            }
        }

        // Property
        [Header("Ghost")]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [Space]
        [SerializeField] private GameObject _swordPrefab;
        [SerializeField] private float _launchStartTime;

        public enum AttackMode
        {
            Any,
            RangedAttack,
            ExplosiveAttack,
        }


        // Internal
        private class GhostBrain : MonsterBrain
        {
            public GhostBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged()
                                .AddChild(new Adjusting(MonsterActionType.Idle))
                                .AddChild(new DeadEnd(MonsterActionType.Idle)))
                            .AddChild(new GhostAttackBrain())
                            .AddChild(new Cooldown()))
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol(MonsterActionType.Idle))))
                    .AddChild(new NotValidPlatform()));
                AddChild(new Dead());
            }
        }

        private class GhostController : MonsterActionController
        {
            public GhostController(Ghost monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent());
                AddChild(new MonsterAction("Attack_1")
                    .AddAnimationComponent(interruptAllOnDeactivate: true)
                    .AddComponent(new AttackWithWeapon(monster._swordPrefab)));
                AddChild(new MonsterAction("Attack_2")
                    .AddComponent(new GhostExplosiveAttackAction()));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }


        // Content
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new GhostController(this);
            ActionController.Enter();

            Brain = new GhostBrain(this);
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(true),
                new(nameof(Hit), new object[] { damageInfo }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
            });
        }
    }
}
