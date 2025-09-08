using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DarkMonsterFirstPhase : Monster
{
    // Internal
    private class DarkMonsterForm1Brain : MonsterBrain
    {
        public DarkMonsterForm1Brain(Monster owner) : base(owner)
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

    private class DarkMonsterForm1Controller : MonsterActionController
    {
        public DarkMonsterForm1Controller(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState(MonsterAction.Walk));
            AddChild(new MonsteActionState(MonsterAction.Attack));
            AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        ActionController = new DarkMonsterForm1Controller(this);
        ActionController.Enter();
        
        Brain = new DarkMonsterForm1Brain(this);
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
    [CustomEditor(typeof(DarkMonsterFirstPhase)), CanEditMultipleObjects]
    private class DarkMonsterForm1Editor : MonsterEditor { }
#endif
}
