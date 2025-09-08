using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Snail : Monster
{
    // Internal
    private class SnailBrain : MonsterBrain
    {
        public SnailBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(true, MonsterAction.Walk))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol())))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class SnailActionController : MonsterActionController
    {
        public SnailActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState(MonsterAction.Alert));
            AddChild(new MonsteActionState(MonsterAction.Walk));
            AddChild(new MonsteActionState(MonsterAction.Run));
            AddChild(new MonsteActionState(MonsterAction.Attack));
            AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

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

#if UNITY_EDITOR
    [CustomEditor(typeof(Snail)), CanEditMultipleObjects]
    private class SnailEditor : MonsterEditor { }
#endif
}
