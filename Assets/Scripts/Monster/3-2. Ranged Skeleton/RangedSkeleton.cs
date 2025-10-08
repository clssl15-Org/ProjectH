using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(StandaloneHitAction), typeof(KinematicProjectileLauncher))]
public class RangedSkeleton : Monster
{
    // Property
    [Header("Ranged Skeleton")]
    [SerializeField, Min(0)] private float launchTime;
    [SerializeField, Min(0)] private float projectileSpeed;


    // Internal
    private KinematicProjectileLauncher projectileLauncher;

    private class RangedkeletonBrain : MonsterBrain
    {
        public RangedkeletonBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged()
                            .AddChild(new Adjusting(MonsterActionType.Walk))
                            .AddChild(new DeadEnd()))
                        .AddChild(new Attack("throw"))
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol())))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class RangedkeletonActionController : MonsterActionController
    {
        public RangedkeletonActionController(RangedSkeleton monster) : base(monster)
        {
            AddChild(new MonsterAction(MonsterActionType.Idle));
            AddChild(new MonsterAction(MonsterActionType.Walk));
            AddChild(new AttackWithKinematicProjectile(
                monster.projectileLauncher,
                () => new(monster.launchTime, monster.projectileSpeed),
                KinematicProjectileLauncher.LaunchType.Directions,
                () => new[] { monster.Direction.ToVector2() },
                "throw"));
            AddChild(new HitFlash());
            AddChild(new MonsterAction(MonsterActionType.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        projectileLauncher = GetComponent<KinematicProjectileLauncher>();
        projectileLauncher.Initialize(this, PlatformManager, "Player", "Ground");

        ActionController = new RangedkeletonActionController(this);
        ActionController.Enter();
        
        Brain = new RangedkeletonBrain(this);
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
