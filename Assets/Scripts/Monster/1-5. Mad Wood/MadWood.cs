using UniEngine.StateMachines.BT;
using MonsterActions;
using MonsterBT;

public partial class MadWood : Monster
{
    // Internal
    public enum AttackMode
    {
        DefaultAttack,
        LandAttack
    }

    private class MadWoodBrain : MonsterBrain
    {
        public MadWoodBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new NotValidPlatform("Fall"))
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(true))
                        .AddChild(new MadWoodAttack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(MonsterAction.Run)))));
            AddChild(new Dead());
        }
    }

    private class MadWoodActionController : MonsterActionController
    {
        public MadWoodActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle.ToString()));
            AddChild(new MonsteActionState("Fall"));
            AddChild(new MonsteActionState(MonsterAction.Run.ToString()));
            AddChild(new MonsteActionState(AttackMode.DefaultAttack.ToString()));
            AddChild(new MonsteActionState(AttackMode.LandAttack.ToString()));
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

        ActionController = new MadWoodActionController(this);
        ActionController.Enter();
        
        Brain = new MadWoodBrain(this);

        // 시작 시 Fall 방지
        // TODO: 추락 State 추가할 것 (플랫폼 미확인과 추락 분리)
        Brain.Tick();
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
