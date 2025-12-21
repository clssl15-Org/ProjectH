using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public class Dokkaebi : Monster<MonsterStats>
    {
        // Property
        [Header("Dokkaebi")]
        [SerializeField] private Weapon _weapon;
        [SerializeField] private float _weaponActiveTiming;
        [Space]
        [SerializeField, Min(0)] private float _targetRangeMin = 0.5f;
        [SerializeField, Min(0)] private float _targetRangeMax = 5f;


        // Internal
        private class DokkaebiBrain : MonsterBrain
        {
            public DokkaebiBrain(Dokkaebi owner) : base(owner)
            {
                var targetAttackRange = (owner._targetRangeMin + owner._targetRangeMax) / 2;
                var tolerance = owner._targetRangeMax - targetAttackRange;

                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(Engaged.RangeType.Ranged)
                                {
                                    TargetAttackRange = targetAttackRange,
                                    UpperRangeTolerance = tolerance,
                                    LowerRangeTolerance = tolerance,
                                }
                                .AddChild(new Adjusting(MonsterActionType.Idle))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack(true))
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol(monsterAction: MonsterActionType.Idle)))
                        )
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class DokkaebiActionController : MonsterActionController
        {
            public DokkaebiActionController(Dokkaebi monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Alert)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(monster._weapon, monster._weaponActiveTiming))
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
            if (!_weapon)
                throw new InvalidOperationException(
                    $"{nameof(Dokkaebi)}은(는) {nameof(_weapon)}을(를) 가지고 있어야 합니다.");

            _weapon.AttackPower = StatsInfo.AttackPower;
            _weapon.gameObject.SetActive(false);

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new DokkaebiActionController(this);
            ActionController.Enter();

            Brain = new DokkaebiBrain(this);
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
