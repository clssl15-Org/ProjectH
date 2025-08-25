using UniEngine.StateMachines.BT;
using MonsterActions;
using MonsterBT;

public partial class ThornySnail : Monster
{
    // Internal
    private class ThornySnailBrain : MonsterBrain
    {
        public ThornySnailBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit(MonsterAction.Dead)) // TODO: 깜빡이로 변경
                .AddChild(new NotValidPlatform())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(monsterAction: MonsterAction.Walk)
                        {
                            TargetAttackRange = 3f,
                            UpperRangeTolerance = 0.1f,
                            LowerRangeTolerance = 0.3f
                        })
                        .AddChild(new Attack()))
                        // 가시달팽이는 Cooldown을 가지지 않습니다.
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol()))));
            AddChild(new Dead());
        }
    }

    private class ThornySnailActionController : MonsterActionController
    {
        public ThornySnailActionController(Monster monster) : base(monster)
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
}
