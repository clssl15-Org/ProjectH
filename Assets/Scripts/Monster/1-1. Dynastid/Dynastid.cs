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
                        .AddChild(new Engaged(Engaged.RangeType.Contact, 1)
                            .AddChild(new Adjusting())
                            .AddChild(new DeadEnd()))
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
            AddChild(new MonsterActionState(MonsterAction.Idle));
            AddChild(new MonsterActionState(MonsterAction.Alert));
            AddChild(new MonsterActionState(MonsterAction.Walk));
            AddChild(new MonsterActionState(MonsterAction.Run));
            AddChild(new MonsterActionState(MonsterAction.Attack));
            AddChild(new MonsterActionState(MonsterAction.Hit));
            AddChild(new MonsterActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        ActionController = new DynastidActionController(this);
        ActionController.Enter();
        
        Brain = new DynastidBrain(this);
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
    [CustomEditor(typeof(Dynastid)), CanEditMultipleObjects]
    private class DynastidEditor : MonsterEditor { }
#endif
}
