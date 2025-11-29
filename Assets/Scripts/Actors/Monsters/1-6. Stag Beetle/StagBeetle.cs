using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class StagBeetle : Monster<StagBeetleStats>
    {
        // Front
        [Header("Stag Beetle")]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [Space]
        [SerializeField, Min(0)] private float _rollingTime = 3f;
        [SerializeField, Min(0)] private float _rollingSpeed = 1f;
        [Space]
        [SerializeField, Min(0)] private float _spikeSpeed;
        [SerializeField, Min(0)] private float _waitingTimeAfterLaunch;

        public enum AttackMode
        {
            Any,
            RollAttack,
            SpikeAttack,
            Roar
        }


        // Internal
        private KinematicProjectileLauncher _spikeLauncher;

        private class StagBeetleBrain : MonsterBrain
        {
            public StagBeetleBrain(StagBeetle stagBeetle) : base(stagBeetle)
            {
                AddChild(new Alive()
                    .AddChild(new Hit(doKnockback: false))
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged()
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd()))
                            .AddChild(new StagBeetleAttackBrain())
                            .AddChild(new Cooldown()))
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol())))
                    .AddChild(new NotValidPlatform()));
                AddChild(new Dead());
            }
        }

        private class StagBeetleActionController : MonsterActionController
        {
            public StagBeetleActionController(StagBeetle stagBeetle) : base(stagBeetle)
            {
                bool rollRight = default;

                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Alert)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(AttackMode.RollAttack.ToString())
                    .AddComponent(new ThreePhasedAction(AttackMode.RollAttack.ToString(),
                        n => n + "Anticipation", n => n + "Recoil",
                        beforePreAction: () => rollRight = stagBeetle.DetectedPlayer.transform.position.x > stagBeetle.transform.position.x,
                        beforeMainAction: () => stagBeetle.IgnorePlayerInteraction = true,
                        whileMainAction: (playtime, _) =>
                        {
                            if (stagBeetle.TryMove())
                                stagBeetle.Rigidbody.velocity = new Vector2
                                {
                                    x = (rollRight ? 1 : -1) * stagBeetle._rollingSpeed,
                                    y = stagBeetle.Rigidbody.velocity.y,
                                };
                            else
                                stagBeetle.StopMoving();

                            return playtime <= stagBeetle._rollingTime;
                        },
                        beforePostAction: () => stagBeetle.IgnorePlayerInteraction = false)));
                AddChild(new MonsterAction(AttackMode.SpikeAttack.ToString())
                    .AddComponent(new ThreePhasedAction(AttackMode.SpikeAttack.ToString(),
                        n => n + "Anticipation", n => n + "Recoil",
                        beforePostAction: () =>
                        {
                            stagBeetle._spikeLauncher.LaunchWithLocalRotation(stagBeetle._spikeSpeed, Vector2.left);
                            stagBeetle.AnimationPlayer.Pause();
                            new Timer(stagBeetle._waitingTimeAfterLaunch, succeed =>
                            {
                                if (!succeed) throw new InvalidOperationException("애니메이션 재생 중 오류가 발생했습니다.");
                                stagBeetle.AnimationPlayer.Resume();
                            });
                        })));
                AddChild(new MonsterAction(AttackMode.Roar.ToString())
                    .AddComponent(new ThreePhasedAction(AttackMode.Roar.ToString(),
                        n => n + "Anticipation", n => n + "Recoil",
                        whileMainAction: (playtime, length) => playtime > length)));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddAnimationComponent("HitGround"));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }


        // Content
        protected override void Awake()
        {
            base.Awake();

            _spikeLauncher = GetComponentInChildren<KinematicProjectileLauncher>(true);

            if (!_spikeLauncher) throw new InvalidOperationException(
                FormatLogMessage($"{nameof(StagBeetle)}은(는) {nameof(_spikeLauncher)} 컴포넌트를 가지고 있어야 합니다."));

            _spikeLauncher.Initialize(this, PlatformManager, "Ground");
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new StagBeetleActionController(this);
            ActionController.Enter();

            Brain = new StagBeetleBrain(this);
            StandaloneHitBrain.DoKnockback = false;
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
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
    }
}
