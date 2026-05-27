using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Bosses;
using Actors.Monsters.Brains;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class StagBeetle : Monster<StagBeetleStats>, IBoss
    {
        // Front
        [Header("Stag Beetle")]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [Space]
        [SerializeField] private Weapon _rollingAttackWeapon;
        [SerializeField, Min(0)] private float _rollingTime = 3f;
        [SerializeField, Min(0)] private float _rollingSpeed = 1f;
        [Space]
        [SerializeField, Min(0)] private float _spikeSpeed;
        [SerializeField, Min(0)] private float _spikeScaleFactor = 0.01f;
        [SerializeField, Min(0)] private float _waitTimeAfterLaunch;
        [Space]
        [SerializeField] private GameObject _roarIndicator;

        [Header("Debug")]
        [SerializeField] private bool _autoAwake = false;

        public bool IsInvincible
        {
            get => IgnorePlayerInteraction;
            set => IgnorePlayerInteraction = value;
        }

        public enum AttackMode
        {
            Any,
            RollAttack,
            SpikeAttack,
            Roar
        }

        public MonsterAudioPlayer AudioPlayer => throw new NotImplementedException();

        private const string IsAwake = nameof(IsAwake);
        private KinematicProjectileLauncher _spikeLauncher;


        // States
        private class StagBeetleBrain : MonsterBrain
        {
            public StagBeetleBrain(StagBeetle owner) : base(owner)
            {
                Blackboard.Properties[IsAwake] = false;

                AddChild(new Alive()
                    .AddChild(new Idle(IsAwake))
                    .AddChild(new Hit(doKnockback: false))
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(Engaged.RangeType.Contact, range: 5f)
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new StagBeetleAttackBrain())
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol()))
                        )
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class StagBeetleActionController : MonsterActionController
        {
            public StagBeetleActionController(StagBeetle monster) : base(monster)
            {
                bool rollRight = default;

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
                AddChild(new MonsterAction(AttackMode.RollAttack.ToString())
                    .AddComponent(new ThreePhasedAction(AttackMode.RollAttack.ToString(),
                        n => n + "Anticipation", n => n + "Recoil",
                        beforePreAction: () => rollRight = monster.DetectedPlayer.transform.position.x > monster.transform.position.x,
                        //beforeMainAction: () => monster.IgnorePlayerInteraction = true,
                        whileMainAction: (playtime, _) =>
                        {
                            if (monster.TryMove())
                                monster.Rigidbody.velocity = new Vector2
                                {
                                    x = (rollRight ? 1 : -1) * monster._rollingSpeed,
                                    y = monster.Rigidbody.velocity.y,
                                };
                            else
                                monster.StopMoving();

                            return playtime <= monster._rollingTime;
                        })
                        //beforePostAction: () => monster.IgnorePlayerInteraction = false)
                        { InterruptPriority = InterruptPriority.High })
                    .AddComponent(new AttackWithWeapon(
                        monster._rollingAttackWeapon,
                        0f,
                        monster._rollingTime))
                );
                AddChild(new MonsterAction(AttackMode.SpikeAttack.ToString())
                    .AddComponent(new ThreePhasedAction(AttackMode.SpikeAttack.ToString(),
                        n => n + "Anticipation", n => n + "Recoil",
                        beforePostAction: () =>
                        {
                            monster._spikeLauncher.LaunchWithLocalRotation(monster._spikeSpeed, Vector2.left);
                            monster.AnimationPlayer.Pause();
                            new Timer(monster._waitTimeAfterLaunch, succeed =>
                            {
                                if (!succeed) throw new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");
                                monster.AnimationPlayer.Resume();
                            });
                        }))
                );
                AddChild(new MonsterAction(AttackMode.Roar.ToString())
                    .AddComponent(new ThreePhasedAction(AttackMode.Roar.ToString(),
                        n => n + "Anticipation", n => n + "Recoil",
                        beforeMainAction: () =>
                        {
                            if (!monster._roarIndicator)
                                return;
                            
                            var indicator = Instantiate(monster._roarIndicator);
                            indicator.transform.position = monster._roarIndicator.transform.position;
                            indicator.SetActive(true);

                            Destroy(
                                indicator,
                                indicator
                                .GetComponent<Animator>()
                                .FindClip("Indicator Roar")
                                .length);
                        },
                        whileMainAction: (playtime, length) => playtime > length)
                    )
                );
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddAnimationComponent("HitGround")
                );
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent()
                );
            }
        }


        // Content
        protected override void Awake()
        {
            base.Awake();

            _spikeLauncher = GetComponentInChildren<KinematicProjectileLauncher>(true);
            if (!_spikeLauncher) throw new InvalidOperationException(
                FormatLogMessage($"{nameof(StagBeetle)}은(는) {nameof(_spikeLauncher)} 컴포넌트를 가지고 있어야 합니다."));

            InitializeSpikeLauncher();

            if (!_rollingAttackWeapon) throw new InvalidOperationException(
                FormatLogMessage($"{nameof(StagBeetle)}은(는) {nameof(_rollingAttackWeapon)} 컴포넌트를 가지고 있어야 합니다."));

            _rollingAttackWeapon.AttackPower = StatsInfo.AttackPower;
        }

        protected override void Start()
        {
            base.Start();
            InitializeSpikeLauncher();

            if (!_roarIndicator)
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_roarIndicator)}이(가) 등록되지 않았습니다."),
                    this);
            else
                _roarIndicator.SetActive(false);

            ActionController = new StagBeetleActionController(this);
            ActionController.Enter();

            Brain = new StagBeetleBrain(this);
            StandaloneHitBrain.DoKnockback = false;

            // ------- Debug -------
            if (_autoAwake.Resolve(false))
                Commence();
        }

        public void Commence() => Brain.Blackboard.Properties[IsAwake] = true;
        
        private void InitializeSpikeLauncher()
        {
            _spikeLauncher
                .Initialize(this, PlatformManager, "Player", "Ground")
                .SetProjectileInitializer(
                    p => p.GetComponent<SpriteSizeHandler>().Initialize(_spikeScaleFactor, true),
                    p => p.GetComponent<Weapon>().AttackPower = StatsInfo.AttackPower);
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            if (!(bool)Brain.Blackboard.Properties[IsAwake]) return;
            StandaloneHitBrain.TryTakeDamage(damageInfo);

            //if (Brain.Blackboard.Committing)
            //    StandaloneHitBrain.TryTakeDamage(damageInfo);
            //else
            //{
            //    StandaloneHitBrain.Stop();
            //    Brain.SelectChild(new SelectionRequest[]
            //    {
            //        new(true),
            //        new(true),
            //        new(nameof(Hit), new object[] { damageInfo }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
            //    });
            //}
        }

        // 사망 후 삭제 방지
        internal override void Die() => Die(false);

        protected override string GetDisplayContent()
        {
            var message = base.GetDisplayContent();

            message += "----------------";
            message += $"\nAwaken: {Brain.Blackboard.Properties[IsAwake]}";

            return message;
        }
    }
}
