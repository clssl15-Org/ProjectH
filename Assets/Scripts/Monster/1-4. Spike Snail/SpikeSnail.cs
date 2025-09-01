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
                        .AddChild(new Engaged(monsterAction: MonsterAction.Walk)
                        {
                            TargetAttackRange = 3f
                        })
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
        public ThornySnailActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState(MonsterAction.Alert));
            AddChild(new MonsteActionState(MonsterAction.Walk));
            AddChild(new MonsteActionState(MonsterAction.Run));
            AddChild(new SpikeSnailAttackAction());
            AddChild(new HitFlash());
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }

    private KinematicProjectileLauncher spikeLauncher;


    // Content
    protected override void Awake()
    {
        base.Awake();
        spikeLauncher = GetComponentInChildren<KinematicProjectileLauncher>(true);

        if (!spikeLauncher) throw new InvalidOperationException(
            "가시달팽이는 spikeLauncher 컴포넌트를 가지고 있어야 합니다.");

        spikeLauncher.Initialize(this, platformManager, "Ground");
    }

    protected void Start()
    {
        Direction = UnityEngine.Random.Range(0, 2) == 0
            ? Direction.Left
            : Direction.Right;

        ActionController = new ThornySnailActionController(this);
        ActionController.Enter();

        Brain = new ThornySnailBrain(this);
    }

    protected override void OnDamaged(int damage)
    {
        Brain.SelectChild(new SelectionRequest[]
        {
            new(true),
            new(true),
            new("Hit", new object[] { damage }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
        });
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SpikeSnail)), CanEditMultipleObjects]
    private class SpikeSnailEditor : MonsterEditor { }
#endif
}
