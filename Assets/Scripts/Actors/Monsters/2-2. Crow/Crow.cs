using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(KinematicProjectileLauncher))]
    public partial class Crow : Monster<MonsterStats>
    {
        // Property
        [Header("Crow")]
        [SerializeField, Min(0)] private float _launchTime;
        [SerializeField, Min(0)] private float _projectileSpeed;


        // Internal
        private KinematicProjectileLauncher _projectileLauncher;

        private class CrowBrain : MonsterBrain
        {
            public CrowBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged()
                                .AddChild(new Adjusting("Fly"))
                                .AddChild(new DeadEnd()))
                            .AddChild(new Attack()))
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol("Fly"))))
                    .AddChild(new NotValidPlatform()));
                AddChild(new Dead());
            }
        }

        private class CrowActionController : MonsterActionController
        {
            public CrowActionController(Crow monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction("Fly")
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelayComponent()
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent()
                    .AddComponent(new AttackWithKinematicProjectile(
                        launcher: monster._projectileLauncher,
                        getLaunchInfo: () => new(monster._launchTime, monster._projectileSpeed),
                        launchType: KinematicProjectileLaunchType.Rotation,
                        getDirections: () => (new Vector2[] { monster.DetectedPlayer.transform.position - monster.transform.position }))));
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

            ActionController = new CrowActionController(this);
            ActionController.Enter();

            Brain = new CrowBrain(this);
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
