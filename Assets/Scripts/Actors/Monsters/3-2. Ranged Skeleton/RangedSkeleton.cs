using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction), typeof(KinematicProjectileLauncher))]
    public class RangedSkeleton : Monster<MonsterStats>
    {
        // Property
        [Header("Ranged Skeleton")]
        [SerializeField, Min(0)] private float _targetRangeMin = 0.5f;
        [SerializeField, Min(0)] private float _targetRangeMax = 5f;
        [Space]
        [SerializeField, Min(0)] private float _launchTime;
        [SerializeField, Min(0)] private float _projectileSpeed;

        private KinematicProjectileLauncher _projectileLauncher;


        // States
        private class RangedkeletonBrain : MonsterBrain
        {
            public RangedkeletonBrain(RangedSkeleton owner) : base(owner)
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
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack(true))
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

        private class RangedkeletonActionController : MonsterActionController
        {
            public RangedkeletonActionController(RangedSkeleton monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(new MonsterAnimationPlayInfo("throw"))
                    .AddComponent(new AttackWithKinematicProjectile(
                        launcher: monster._projectileLauncher,
                        getLaunchInfo: () => new(monster._launchTime, monster._projectileSpeed),
                        launchType: KinematicProjectileLaunchType.Directions,
                        getDirections: () => new[] { Vector2.left }) // HACK: 왜 왼쪽? 작동하니 두기
                    )
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

            _projectileLauncher = GetComponent<KinematicProjectileLauncher>();
            _projectileLauncher
                .Initialize(this, PlatformManager)
                .SetProjectileInitializer(
                    p => p.GetComponent<Weapon>().AttackPower = StatsInfo.AttackPower);

            ActionController = new RangedkeletonActionController(this);
            ActionController.Enter();

            Brain = new RangedkeletonBrain(this);
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
