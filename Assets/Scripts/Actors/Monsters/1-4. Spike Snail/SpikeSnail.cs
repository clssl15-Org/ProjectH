using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class SpikeSnail : Monster<MonsterStats>
    {
        // Property
        [Header("Spike Snail")]
        [SerializeField, Min(0)] internal float spikeSpeed;
        [SerializeField, Min(0)] private float launchTime;
        [SerializeField, Min(0)] private float playtimeBeforeWaiting;
        [SerializeField, Min(0)] private float waitingTime;


        // Internal
        private class SpikeSnailBrain : MonsterBrain
        {
            public SpikeSnailBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged()
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd()))
                            .AddChild(new Attack())
                            .AddChild(new Cooldown(MonsterActionType.None)))
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol())))
                    .AddChild(new NotValidPlatform()));
                AddChild(new Dead());
            }
        }

        private class SpikeSnailActionController : MonsterActionController
        {
            public SpikeSnailActionController(SpikeSnail monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent(new MonsterAnimationPlayInfo(MonsterActionType.Idle, startTime: 0.33f, endTime: 2.08f)));
                AddChild(new MonsterAction(MonsterActionType.Alert)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent()
                    .AddComponent(new AttackWithKinematicProjectile(
                        launcher: monster._spikeLauncher,
                        getLaunchInfo: () => new(monster.launchTime, monster.spikeSpeed),
                        launchType: KinematicProjectileLaunchType.Directions,
                        getDirections: () => (new Vector2[] { new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0) }))));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }

        private KinematicProjectileLauncher _spikeLauncher;


        // Content
        protected override void Awake()
        {
            base.Awake();
            _spikeLauncher = GetComponentInChildren<KinematicProjectileLauncher>(true);

            if (!_spikeLauncher) throw new InvalidOperationException(FormatLogMessage(
                $"{nameof(SpikeSnail)}은(는) {nameof(_spikeLauncher)} 컴포넌트를 가지고 있어야 합니다."));

            _spikeLauncher.Initialize(this, PlatformManager, "Ground");
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new SpikeSnailActionController(this);
            ActionController.Enter();

            Brain = new SpikeSnailBrain(this);
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
