using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(StandaloneHitAction))]
public class MeleeSkeleton : Monster
{
    // Internal
    private class MeleeSkeletonBrain : MonsterBrain
    {
        public MeleeSkeletonBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(Engaged.RangeType.Contact)
                            .AddChild(new Adjusting(MonsterActionType.Walk))
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

    private class MeleeSkeletonActionController : MonsterActionController
    {
        public MeleeSkeletonActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsterAction(MonsterActionType.Idle));
            AddChild(new MonsterAction(MonsterActionType.Walk));
            AddChild(new MonsterAction(MonsterActionType.Attack));
            AddChild(new HitFlash());
            AddChild(new MonsterAction(MonsterActionType.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        ActionController = new MeleeSkeletonActionController(this);
        ActionController.Enter();
        
        Brain = new MeleeSkeletonBrain(this);
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
    [CustomEditor(typeof(MeleeSkeleton)), CanEditMultipleObjects]
    private class MeleeSkeletonEditor : MonsterEditor { }
#endif
}
