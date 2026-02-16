using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public class MeleeSkeleton : Monster<MonsterStats>
    {
        [Header("Melee Skeleton")]
        [SerializeField] private Weapon _weapon;

        // Stats
        private class MeleeSkeletonBrain : MonsterBrain
        {
            public MeleeSkeletonBrain(MeleeSkeleton owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(
                                    Engaged.RangeType.Ranged,
                                    Mathf.Abs((
                                        owner._weapon?.transform.localPosition.x
                                        ?? Engaged.DefaultTargetAttackRange)
                                        * owner.transform.lossyScale.z)
                                )
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack(false))
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol())
                        )
                    )
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class MeleeSkeletonActionController : MonsterActionController
        {
            public MeleeSkeletonActionController(MeleeSkeleton monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(
                        monster._weapon,
                        onInstantiate: weapon => weapon.SetKnockbackInfo(() =>
                        {
                            if (!monster) return null;
                            return monster.Direction;
                        }))
                    )
                );
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
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

            ActionController = new MeleeSkeletonActionController(this);
            ActionController.Enter();

            Brain = new MeleeSkeletonBrain(this);
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
