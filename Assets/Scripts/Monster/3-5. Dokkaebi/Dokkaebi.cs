using System;
using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
                        .AddChild(new Patrol(monsterAction: MonsterAction.Idle))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class DokkaebiActionController : MonsterActionController
    {
        public DokkaebiActionController(Monster monster) : base(monster)
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

    protected override void Start()
    {
        base.Start();

        ActionController = new DokkaebiActionController(this);
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

#if UNITY_EDITOR
    [CustomEditor(typeof(Dokkaebi)), CanEditMultipleObjects]
    private class DokkaebiEditor : MonsterEditor { }
#endif
}
