using System;
using UnityEngine;
using UniEngine.StateMachines.BT;
using MonsterActions;
using MonsterBT;

public partial class JavelinHurler : Monster
{
    // Property
    [Header("Javelin Hurler")]
    [SerializeField] private GameObject javelinPrefab;
    [SerializeField, Min(0)] private float javelinScale = 1;
    [SerializeField] private Vector2 javelinPosition;
    [SerializeField, Min(0)] private float throwTime = 1;
    [SerializeField, Range(0, 90)] private float throwAngle;
    [SerializeField, Min(0)] private float throwPower;


    // Internal
    private class JavelinHurlerBrain : MonsterBrain
    {
        public JavelinHurlerBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new NotValidPlatform())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(monsterAction: MonsterAction.Walk)
                        {
                            TargetAttackRange = 5f,
                            UpperRangeTolerance = 0.1f,
                            LowerRangeTolerance = 0.3f
                        })
                        .AddChild(new Attack()))
                        // 창던지개는 Cooldown을 가지지 않습니다.
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol()))));
            AddChild(new Dead());
        }
    }

    private class JavelinHurlerActionController : MonsterActionController
    {
        public JavelinHurlerActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Alert.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Walk.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Run.ToString()));
            AddChild(new JavelinHurlerAttackAction());
            AddChild(new MonsteActionState(MonsterAction.Hit.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Dead.ToString()));
        }
    }


    // Content
    protected override void Awake()
    {
        if (!javelinPrefab)
            throw new InvalidOperationException("창던지개는 javelinPrefab을 가지고 있어야 합니다.");

        base.Awake();
    }

    protected void Start()
    {
        Direction = UnityEngine.Random.Range(0, 2) == 0
            ? Direction.Left
            : Direction.Right;

        ActionController = new JavelinHurlerActionController(this);
        ActionController.Enter();

        Brain = new JavelinHurlerBrain(this);
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
