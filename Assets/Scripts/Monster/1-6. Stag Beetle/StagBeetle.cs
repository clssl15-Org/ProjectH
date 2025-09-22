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
    [Space()]
    [SerializeField, Min(0)] private float rollingTime = 3f;
    [SerializeField, Min(0)] private float rollingSpeed = 1f;
    [Space()]
    [SerializeField, Min(0)] internal float spikeSpeed;
    [SerializeField, Min(0)] private float launchTime;
    [SerializeField, Min(0)] private float staytimeBeforeContinue;
    [SerializeField, Min(0)] private float waitingTime;

    public enum AttackMode
    {
        Any,
        RollAttack,
        SpikeAttack,
        Roar
    }


    // Internal
    private KinematicProjectileLauncher spikeLauncher;

    private class StagBeetleBrain : MonsterBrain
    {
        public StagBeetleBrain(StagBeetle stagBeetle) : base(stagBeetle)
        {
            AddChild(new Alive()
                .AddChild(new Hit("HitGround"))
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged()
                            .AddChild(new Adjusting(MonsterAction.Walk))
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


            AddChild(new MonsterActionState(MonsterAction.Idle));
            AddChild(new MonsterActionState(MonsterAction.Alert));
            AddChild(new MonsterActionState(MonsterAction.Walk));
            AddChild(new MonsterActionState(MonsterAction.Run));
            AddChild(new ThreePhasedAction(AttackMode.RollAttack.ToString(),
                n => n + "Anticipation", n => n + "Recoil",
                beforePreAction: () => rollRight = stagBeetle.DetectedPlayer.transform.position.x > stagBeetle.transform.position.x,
                beforeMainAction: () => stagBeetle.Collider.excludeLayers = LayerMask.GetMask("Player"),
                whileMainAction: (playtime, _) =>
                {
                    if (stagBeetle.TryMove())
                        stagBeetle.Rigidbody.velocity = new Vector2
                        {
                            x = (rollRight ? 1 : -1) * stagBeetle.rollingSpeed,
                            y = stagBeetle.Rigidbody.velocity.y,
                        };
                    else
                        stagBeetle.StopMoving();

                    return playtime > stagBeetle.rollingTime;
                },
                afterMainAction: () => stagBeetle.Collider.excludeLayers = default));
            AddChild(new ThreePhasedAction(AttackMode.SpikeAttack.ToString(),
                n => n + "Anticipation", n => n + "Recoil",
                beforeMainAction: () =>
                {
                    spikeLaunched = false;
                    restarted = false;
                },
                whileMainAction: (playtime, lentgh) =>
                {
                    if (!spikeLaunched && playtime >= stagBeetle.launchTime)
                    {
                        spikeLaunched = true;

                        stagBeetle.spikeLauncher.LaunchWithLocalRotation(stagBeetle.spikeSpeed, Vector2.left);
                        stagBeetle.Animator.speed = 0f;
                    }

                    if (!restarted && playtime >= stagBeetle.launchTime + stagBeetle.staytimeBeforeContinue)
                    {
                        restarted = true;
                        stagBeetle.Animator.speed = 1f;
                    }

                    return playtime > lentgh + stagBeetle.waitingTime + stagBeetle.staytimeBeforeContinue;
                }));
            AddChild(new ThreePhasedAction(AttackMode.Roar.ToString(),
                n => n + "Anticipation", n => n + "Recoil",
                whileMainAction: (playtime, length) => playtime > length));
            AddChild(new MonsterActionState("HitGround"));
            AddChild(new MonsterActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Awake()
    {
        base.Awake();

        spikeLauncher = GetComponentInChildren<KinematicProjectileLauncher>(true);

        if (!spikeLauncher) throw new InvalidOperationException(
            Ctx("이 몬스터는 SpikeLauncher 컴포넌트를 가지고 있어야 합니다."));

        spikeLauncher.Initialize(this, platformManager, "Ground");
    }

    protected override void Start()
    {
        base.Start();

        ActionController = new StagBeetleActionController(this);
        ActionController.Enter();
        
        Brain = new StagBeetleBrain(this);
    }

    protected override void OnDamaged(int damage)
    {
        if (Brain.Blackboard.Committing)
            StandaloneHitBrain.TryTakeDamage(damage);
        else
        {
            StandaloneHitBrain.Stop();
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(true),
                new("Hit", new object[] { damage }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
            });
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(StagBeetle)), CanEditMultipleObjects]
    private class StagBeetleEditor : MonsterEditor { }
#endif
}
