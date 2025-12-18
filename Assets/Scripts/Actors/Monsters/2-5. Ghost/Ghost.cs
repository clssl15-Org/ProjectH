using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
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
        [SerializeField] private Weapon _swordPrefab;
        [SerializeField] private float _launchStartTime;
        [Space]
        [SerializeField] private Weapon _rangedWeaponPrefab;

        public enum AttackMode
        {
            Any,
            RangedAttack,
            ExplosiveAttack,
        }


        // States
        private class GhostBrain : MonsterBrain
        {
            public GhostBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(Engaged.RangeType.Ranged)
                                {
                                    TargetAttackRange = 3f,
                                    UpperRangeTolerance = 2f,
                                }
                                .AddChild(new Adjusting(MonsterActionType.Idle))
                                .AddChild(new DeadEnd(MonsterActionType.Idle))
                            )
                            .AddChild(new GhostAttackBrain())
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol(MonsterActionType.Idle))
                        )
                    )
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class GhostController : MonsterActionController
        {
            public GhostController(Ghost monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction("Attack_1")
                    .AddAnimationComponent(interruptAllOnDeactivate: true)
                    .AddComponent(new AttackWithWeapon(monster._swordPrefab))
                );
                AddChild(new MonsterAction("Attack_2")
                    .AddComponent(new GhostExplosiveAttackAction())
                );
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent()
                );
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

            _swordPrefab.AttackPower = StatsInfo.RangedAttackPower;
            _swordPrefab.GetComponent<SpriteSizeHandler>().Initialize(Configuration, true);

            _rangedWeaponPrefab.AttackPower = StatsInfo.AttackPower;

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
