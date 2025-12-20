using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public class DarkMonsterSecondPhase : Monster<MonsterStats>
    {
        // Property
        [Header("Dark Monster Second Phase")]
        [SerializeField] private Weapon _weapon;
        [SerializeField, Min(0)] private float _weaponActiveTiming;
        [SerializeField] private float _weaponActiveDuration;


        // States
        private class DarkMonsterSecondPhaseBrain : MonsterBrain
        {
            public DarkMonsterSecondPhaseBrain(DarkMonsterSecondPhase owner) : base(owner)
            {
                AddChild(new Alive()
                    //.AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(
                                    Engaged.RangeType.Ranged,
                                    Mathf.Abs(
                                        owner._weapon?.transform.localPosition.x
                                        ?? Engaged.DefaultTargetAttackRange)
                                )
                                .AddChild(new Adjusting(MonsterActionType.Idle))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack())
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

        private class DarkMonsterSecondPhaseController : MonsterActionController
        {
            public DarkMonsterSecondPhaseController(DarkMonsterSecondPhase monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(
                        monster._weapon,
                        monster._weaponActiveTiming,
                        monster._weaponActiveDuration)
                    )
                );
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddAnimationComponent()
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
                _weapon.AttackPower = StatsInfo.AttackPower;
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_weapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            ActionController = new DarkMonsterSecondPhaseController(this);
            ActionController.Enter();

            Brain = new DarkMonsterSecondPhaseBrain(this);
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            StandaloneHitBrain.TryTakeDamage(damageInfo);
        }
    }
}
