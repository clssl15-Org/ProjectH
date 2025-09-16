using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(KinematicProjectileLauncher))]
public partial class Crow : Monster
{
    // Property
    [Header("Crow")]
    [SerializeField, Min(0)] private float launchTime;
    [SerializeField, Min(0)] private float projectileSpeed;


    // Internal
    private KinematicProjectileLauncher projectileLauncher;

    private class CrowBrain : MonsterBrain
    {
        public CrowBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Adjusting(false, "Fly")
                        {
                            TargetAttackRange = 3f,
                            UpperRangeTolerance = 0.1f,
                            LowerRangeTolerance = 0.3f
                        })
                        .AddChild(new Attack()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol("Fly"))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class CrowActionController : MonsterActionController
    {
        public CrowActionController(Crow monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState("Fly"));
            AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new AttackWithKinematicProjectile(
                monster.projectileLauncher,
                () => new(monster.launchTime, monster.projectileSpeed),
                KinematicProjectileLauncher.LaunchType.Rotation,
                () => new Vector2[] { monster.DetectedPlayer.transform.position - monster.transform.position }));
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        projectileLauncher = GetComponent<KinematicProjectileLauncher>();
        projectileLauncher.Initialize(this, platformManager, "Player", "Ground");

        ActionController = new CrowActionController(this);
        ActionController.Enter();

        Brain = new CrowBrain(this);
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
    [CustomEditor(typeof(Crow)), CanEditMultipleObjects]
    private class CrowEditor : MonsterEditor { }
#endif
}
