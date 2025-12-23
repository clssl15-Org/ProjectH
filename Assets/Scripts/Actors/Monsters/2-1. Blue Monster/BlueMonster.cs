using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public class BlueMonster : Monster<MonsterStats>
    {
        [Header("Blue Monster")]
        [SerializeField] private Weapon _weapon;
        [SerializeField, Min(0)] private float _weaponActiveTiming;
        [SerializeField] private float _weaponActiveDuration;


        // Internal
        private class BlueMonsterBrain : MonsterBrain
        {
            public BlueMonsterBrain(BlueMonster owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(
                                    Engaged.RangeType.Ranged,
                                    Mathf.Abs(
                                        owner._weapon?.transform.localPosition.x
                                        ?? Engaged.DefaultTargetAttackRange)
                                )
                                .AddChild(new Adjusting())
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack(false))
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol(MonsterActionType.Run))
                        )
                    )
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class BlueMonsterActionController : MonsterActionController
        {
            public BlueMonsterActionController(BlueMonster monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(
                        monster._weapon,
                        monster._weaponActiveTiming,
                        monster._weaponActiveDuration))
                );
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddComponent(new HitFlash())
                );
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent()
                );
            }
        }


        // Content
        protected override void Start()
        {
            base.Start();

            if (_weapon)
            {
                _weapon.AttackPower = StatsInfo.AttackPower;
                _weapon.gameObject.SetActive(false);
            }
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_weapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            ActionController = new BlueMonsterActionController(this);
            ActionController.Enter();

            Brain = new BlueMonsterBrain(this);
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(true),
                new(nameof(Hit), new object[] { damageInfo }, EntryPolicy.Unconditional, RerunPolicy.Restart)
            });
        }
    }
}
