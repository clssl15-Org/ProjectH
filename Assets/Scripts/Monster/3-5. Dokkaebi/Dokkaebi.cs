using System;
using UnityEngine;
using UniEngine.StateMachines.BT;
using MonsterActions;
using MonsterBT;

public partial class Dokkaebi : Monster
{
    // Property
    [Header("Dokkaebi")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Vector2 laserPosition;


    // Internal
    private class DokkaebiBrain : MonsterBrain
    {
        public DokkaebiBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new NotValidPlatform())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(monsterAction: MonsterAction.Idle)
                        {
                            TargetAttackRange = 3f,
                            UpperRangeTolerance = 0.1f,
                            LowerRangeTolerance = 0.3f
                        })
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(monsterAction: MonsterAction.Idle)))));
            AddChild(new Dead());
        }
    }

    private class DynastidActionController : MonsterActionController
    {
        public DynastidActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Alert.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Walk.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Run.ToString()));
            AddChild(new AttackAction());
            AddChild(new MonsteActionState(MonsterAction.Hit.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Dead.ToString()));
        }
    }


    // Content
    protected override void Awake()
    {
        if (!laserPrefab)
            throw new InvalidOperationException("도깨비는 laserPrefab을 가지고 있어야 합니다.");

        base.Awake();
    }

    protected void Start()
    {
        Direction = UnityEngine.Random.Range(0, 2) == 0
            ? Direction.Left
            : Direction.Right;

        ActionController = new DynastidActionController(this);
        ActionController.Enter();

        Brain = new DokkaebiBrain(this);
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
}
