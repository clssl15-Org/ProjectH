using System;
using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(StandaloneHitAction))]
public partial class StagBeetle : Monster
{
    // Front
    [Header("Stag Beetle")]
    [SerializeField] private AttackMode _attackMode = AttackMode.Any;
    [Space]
    [SerializeField, Min(0)] private float _rollingTime = 3f;
    [SerializeField, Min(0)] private float _rollingSpeed = 1f;
    [Space]
    [SerializeField, Min(0)] internal float SpikeSpeed;
    [SerializeField, Min(0)] private float _launchTime;
    [SerializeField, Min(0)] private float _staytimeBeforeContinue;
    [SerializeField, Min(0)] private float _waitingTime;

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
                        .AddChild(new StagBeetleAttack())
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

            bool spikeLaunched = false;
            bool restarted = false;


            AddChild((object)new MonsterAction(MonsterActionType.Idle)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Alert)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Walk)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Run)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(AttackMode.RollAttack.ToString())
                .AddComponent(new ThreePhasedAction(AttackMode.RollAttack.ToString(),
                    n => n + "Anticipation", n => n + "Recoil",
                    beforePreAction: () => rollRight = stagBeetle.DetectedPlayer.transform.position.x > stagBeetle.transform.position.x,
                    beforeMainAction: () => stagBeetle.Collider.excludeLayers = LayerMask.GetMask("Player"),
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
                    afterMainAction: () => stagBeetle.Collider.excludeLayers = default)));
            AddChild((object)new MonsterAction(AttackMode.SpikeAttack.ToString())
                .AddComponent(new ThreePhasedAction(AttackMode.SpikeAttack.ToString(),
                    n => n + "Anticipation", n => n + "Recoil",
                    beforeMainAction: () =>
                    {
                        spikeLaunched = false;
                        restarted = false;
                    },
                    whileMainAction: (playtime, lentgh) =>
                    {
                        if (!spikeLaunched && playtime >= stagBeetle._launchTime)
                        {
                            spikeLaunched = true;

                            stagBeetle._spikeLauncher.LaunchWithLocalRotation(stagBeetle.SpikeSpeed, Vector2.left);
                            stagBeetle.AnimationPlayer.Pause();
                        }

                        if (!restarted && playtime >= stagBeetle._launchTime + stagBeetle._staytimeBeforeContinue)
                        {
                            restarted = true;
                            stagBeetle.AnimationPlayer.Resume();
                        }

                        return playtime <= lentgh + stagBeetle._waitingTime + stagBeetle._staytimeBeforeContinue;
                    })));
            AddChild((object)new MonsterAction(AttackMode.Roar.ToString())
                .AddComponent(new ThreePhasedAction(AttackMode.Roar.ToString(),
                    n => n + "Anticipation", n => n + "Recoil",
                    whileMainAction: (playtime, length) => playtime > length)));
            AddChild((object)new MonsterAction(MonsterActionType.Hit)
                .AddAnimationComponent(new PlayInfo("HitGround")));
            AddChild((object)new MonsterAction(MonsterActionType.Dead)
                .AddAnimationComponent());
        }
    }


    // Content
    protected override void Awake()
    {
        base.Awake();

        _spikeLauncher = GetComponentInChildren<KinematicProjectileLauncher>(true);

        if (!_spikeLauncher) throw new InvalidOperationException(
            Ctx($"{nameof(StagBeetle)}은(는) {nameof(_spikeLauncher)} 컴포넌트를 가지고 있어야 합니다."));

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
        if (Brain.Blackboard.Committing)
            StandaloneHitBrain.TryTakeDamage(damageInfo);
        else
        {
            StandaloneHitBrain.Stop();
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(true),
                new("Hit", new object[] { damageInfo }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
            });
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(StagBeetle)), CanEditMultipleObjects]
    private class StagBeetleEditor : MonsterEditor { }
#endif
}
