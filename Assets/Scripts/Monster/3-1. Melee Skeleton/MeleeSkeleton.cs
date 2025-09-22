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
                            .AddChild(new Adjusting(MonsterAction.Walk))
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
            AddChild(new MonsterActionState(MonsterAction.Idle));
            AddChild(new MonsterActionState(MonsterAction.Walk));
            AddChild(new MonsterActionState(MonsterAction.Attack));
            AddChild(new HitFlash());
            AddChild(new MonsterActionState(MonsterAction.Dead));
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
    [CustomEditor(typeof(MeleeSkeleton)), CanEditMultipleObjects]
    private class MeleeSkeletonEditor : MonsterEditor { }
#endif
}
