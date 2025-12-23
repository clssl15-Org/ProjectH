using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class FireMonster : Monster<MonsterStats>
    {
        // Property
        [Header("Fire Monster")]
        [SerializeField] private Weapon _firePrefab;
        [SerializeField, Min(0)] private float _fireAppearTime;


        // Internal
        private class FireMonsterBrain : MonsterBrain
        {
            public FireMonsterBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit(doKnockback: false))
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            //.AddChild(new Engaged(Engaged.RangeType.Contact, 2f)
                            //    .AddChild(new Adjusting(MonsterActionType.Idle))
                            //    .AddChild(new DeadEnd()))
                            .AddChild(new LookPlayerBrain())
                            .AddChild(new Attack(false))
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                        )
                    )
                    //.AddChild(new Patrol(monsterAction: MonsterActionType.Idle))))
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class FireMonsterActionController : MonsterActionController
        {
            public FireMonsterActionController(FireMonster monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(monster._firePrefab, monster._fireAppearTime))
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
            if (!_firePrefab)
                throw new InvalidOperationException(
                    $"{nameof(FireMonster)}은(는) {nameof(_firePrefab)}을(를) 가지고 있어야 합니다.");

            _firePrefab.AttackPower = StatsInfo.AttackPower;
            _firePrefab.gameObject.SetActive(false);

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new FireMonsterActionController(this);
            ActionController.Enter();

            Brain = new FireMonsterBrain(this);
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
