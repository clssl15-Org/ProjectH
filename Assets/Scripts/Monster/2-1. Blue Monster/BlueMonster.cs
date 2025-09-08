using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(StandaloneHitAction))]
public class BlueMonster : Monster
{
    // Internal
    private class BlueMonsterBrain : MonsterBrain
    {
        public BlueMonsterBrain(Monster owner) : base(owner)
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
                        .AddChild(new Patrol(MonsterAction.Run))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class BlueMonsterActionController : MonsterActionController
    {
        public BlueMonsterActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState(MonsterAction.Run));
            AddChild(new MonsteActionState(MonsterAction.Attack));
            AddChild(new HitFlash());
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        ActionController = new BlueMonsterActionController(this);
        ActionController.Enter();
        
        Brain = new BlueMonsterBrain(this);
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
    [CustomEditor(typeof(BlueMonster)), CanEditMultipleObjects]
    private class BlueMonsterEditor : MonsterEditor { }
#endif
}
