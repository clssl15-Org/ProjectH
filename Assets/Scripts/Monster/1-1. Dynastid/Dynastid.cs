using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Dynastid : Monster
{
    // Internal
    private class DynastidBrain : MonsterBrain
    {
        public DynastidBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(true))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol())))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class DynastidActionController : MonsterActionController
    {
        public DynastidActionController(Monster monster) : base(monster)
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
    protected void Start()
    {
        Direction = UnityEngine.Random.Range(0, 2) == 0
            ? Direction.Left
            : Direction.Right;

        ActionController = new DynastidActionController(this);
        ActionController.Enter();
        
        Brain = new DynastidBrain(this);
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
    [CustomEditor(typeof(Dynastid)), CanEditMultipleObjects]
    private class DynastidEditor : MonsterEditor { }
#endif
}
