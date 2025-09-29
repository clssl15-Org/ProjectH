using System;
using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(StandaloneHitAction))]
public partial class SpikeSnail : Monster
{
    // Property
    [Header("Spike Snail")]
    [SerializeField, Min(0)] internal float spikeSpeed;
    [SerializeField, Min(0)] private float launchTime;
    [SerializeField, Min(0)] private float playtimeBeforeWaiting;
    [SerializeField, Min(0)] private float waitingTime;


    // Internal
    private class ThornySnailBrain : MonsterBrain
    {
        public ThornySnailBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(range: 3f)
                            .AddChild(new Adjusting(MonsterAction.Walk))
                            .AddChild(new DeadEnd()))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown(MonsterAction.None)))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol())))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class ThornySnailActionController : MonsterActionController
    {
        public ThornySnailActionController(SpikeSnail monster) : base(monster)
        {
            AddChild(new MonsterActionState(MonsterAction.Idle, start: 0.33f, end: 2.08f));
            AddChild(new MonsterActionState(MonsterAction.Alert));
            AddChild(new MonsterActionState(MonsterAction.Walk));
            AddChild(new MonsterActionState(MonsterAction.Run));
            AddChild(new AttackWithKinematicProjectile(
                monster.spikeLauncher,
                () => new(monster.launchTime, monster.spikeSpeed),
                KinematicProjectileLauncher.LaunchType.Directions,
                () => new Vector2[] { new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0) }));
            AddChild(new HitFlash());
            AddChild(new MonsterActionState(MonsterAction.Dead));
        }
    }

    private KinematicProjectileLauncher spikeLauncher;


    // Content
    protected override void Awake()
    {
        base.Awake();
        spikeLauncher = GetComponentInChildren<KinematicProjectileLauncher>(true);

        if (!spikeLauncher) throw new InvalidOperationException(
            Ctx("이 몬스터는 spikeLauncher 컴포넌트를 가지고 있어야 합니다."));

        spikeLauncher.Initialize(this, PlatformManager, "Ground");
    }

    protected override void Start()
    {
        base.Start();

        ActionController = new ThornySnailActionController(this);
        ActionController.Enter();

        Brain = new ThornySnailBrain(this);
    }

    protected override void OnDamaged(DamageInfo damageInfo)
    {
        Brain.SelectChild(new SelectionRequest[]
        {
            new(true),
            new(true),
            new("Hit", new object[] { damageInfo }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
        });
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SpikeSnail)), CanEditMultipleObjects]
    private class SpikeSnailEditor : MonsterEditor { }
#endif
}
