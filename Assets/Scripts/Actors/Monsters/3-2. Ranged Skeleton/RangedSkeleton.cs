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
        [SerializeField, Min(0)] private float _launchTime;
        [SerializeField, Min(0)] private float _projectileSpeed;


        // Internal
        private KinematicProjectileLauncher _projectileLauncher;

        private class RangedkeletonBrain : MonsterBrain
        {
            public RangedkeletonBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged()
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd()))
                            .AddChild(new Attack())
                            .AddChild(new Cooldown()))
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol())))
                    .AddChild(new NotValidPlatform()));
                AddChild(new Dead());
            }
        }

        private class RangedkeletonActionController : MonsterActionController
        {
            public RangedkeletonActionController(RangedSkeleton monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(new MonsterAnimationPlayInfo("throw"))
                    .AddComponent(new AttackWithKinematicProjectile(
                        launcher: monster._projectileLauncher,
                        getLaunchInfo: () => new(monster._launchTime, monster._projectileSpeed),
                        launchType: KinematicProjectileLaunchType.Directions,
                        getDirections: () => (new[] { monster.Direction.ToVector2() }))));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelayComponent()
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }


        // Content
        protected override void Start()
        {
            base.Start();

            _projectileLauncher = GetComponent<KinematicProjectileLauncher>();
            _projectileLauncher.Initialize(this, PlatformManager, "Player", "Ground");

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
