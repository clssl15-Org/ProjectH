using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;
using World;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class SpikeSnail : Monster<MonsterStats>
    {
        // Property
        [Header("Spike Snail")]
        [SerializeField] WeaponManager _weaponManager;
        [SerializeField, Min(0)] internal float _spikeSpeed;
        [SerializeField, Min(0)] private float _launchTime;
        [SerializeField, Min(0)] private float _playtimeBeforeWaiting;
        [SerializeField, Min(0)] private float _waitingTime;


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
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack(true))
                            .AddChild(new Cooldown(MonsterActionType.None))
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

        private class SpikeSnailActionController : MonsterActionController
        {
            public SpikeSnailActionController(SpikeSnail monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent(new MonsterAnimationPlayInfo(MonsterActionType.Idle, StartTime: 0.33f, EndTime: 2.08f)));
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
                        getLaunchInfo: () => new(monster._launchTime, monster._spikeSpeed),
                        launchType: KinematicProjectileLaunchType.Directions,
                        getDirections: () => (new Vector2[] { new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0) }))));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
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

            if (_weaponManager)
                _weaponManager.AttackPower = StatsInfo.AttackPower;
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_weaponManager)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            _spikeLauncher = GetComponentInChildren<KinematicProjectileLauncher>(true);
            if (!_spikeLauncher) throw new InvalidOperationException(FormatLogMessage(
                $"{nameof(SpikeSnail)}은(는) {nameof(_spikeLauncher)} 컴포넌트를 가지고 있어야 합니다."));

            InitializeComponents();
        }

        public override void Initialize(
            GameAssetLibrary gameAssetsLibrary,
            Configuration configuration,
            PlatformManager platformManager)
        {
            base.Initialize(gameAssetsLibrary, configuration, platformManager);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _spikeLauncher
                .Initialize(this, PlatformManager, "Ground", "Player")
                .SetProjectileInitializer(
                    p => p.GetComponent<SpriteSizeHandler>().Initialize(Configuration, true));
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
