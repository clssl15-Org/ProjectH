using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class EyeballMonsterSecondPhase : Monster<MonsterStats>
    {
        // Property
        [Header("Eyeball Monster Second Phase")]
        [SerializeField, Min(0)] private float _minDelayAfterBorn = 0f;
        [SerializeField, Min(0)] private float _maxDelayAfterBorn = 2f;
        [Space]
        [SerializeField] private Weapon _weapon;
        [SerializeField, Min(0)] private float _weaponActiveTiming;
        [SerializeField] private float _weaponActiveDuration;


        // States
        private class EyeballMonsterSecondPhaseBrain : MonsterBrain
        {
            public EyeballMonsterSecondPhaseBrain(EyeballMonsterSecondPhase owner) : base(owner)
            {
                AddChild(new SecondPhaseBornBrain());
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
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack())
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol(MonsterActionType.Walk))
                        )
                    )
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class EyeballMonsterSecondPhaseController : MonsterActionController
        {
            public EyeballMonsterSecondPhaseController(EyeballMonsterSecondPhase monster) : base(monster)
            {
                AddChild(new MonsterAction("Born")
                    .AddAnimationComponent("appear")
                );
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(interruptAllOnDeactivate: true)
                    .AddComponent(new AttackWithWeapon(
                        monster._weapon,
                        monster._weaponActiveTiming,
                        monster._weaponActiveDuration))
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

            ActionController = new EyeballMonsterSecondPhaseController(this);
            ActionController.Enter();

            Brain = new EyeballMonsterSecondPhaseBrain(this);
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            StandaloneHitBrain.TryTakeDamage(damageInfo);
        }
    }
}
