using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;

public class Snail : Monster
{
    // Internal
    private class SnailBrain : MonsterBrain
    {
        public SnailBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new NotValidPlatform())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(true, MonsterAction.Walk))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol()))));
            AddChild(new Dead());
        }
    }

    private class SnailActionController : MonsterActionController
    {
        public SnailActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Alert.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Walk.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Run.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Attack.ToString()));
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

        ActionController = new SnailActionController(this);
        ActionController.Enter();
        
        Brain = new SnailBrain(this);
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
