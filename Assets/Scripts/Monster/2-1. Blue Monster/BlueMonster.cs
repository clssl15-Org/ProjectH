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
                        .AddChild(new Engaged(Engaged.RangeType.Contact)
                            .AddChild(new Adjusting())
                            .AddChild(new DeadEnd()))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(MonsterActionType.Run))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class BlueMonsterActionController : MonsterActionController
    {
        public BlueMonsterActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsterAction(MonsterActionType.Idle));
            AddChild(new MonsterAction(MonsterActionType.Run));
            AddChild(new MonsterAction(MonsterActionType.Attack));
            AddChild(new HitFlash());
            AddChild(new MonsterAction(MonsterActionType.Dead));
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
    [CustomEditor(typeof(BlueMonster)), CanEditMultipleObjects]
    private class BlueMonsterEditor : MonsterEditor { }
#endif
}
