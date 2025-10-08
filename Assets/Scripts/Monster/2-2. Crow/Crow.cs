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
    [SerializeField, Min(0)] private float _launchTime;
    [SerializeField, Min(0)] private float _projectileSpeed;


    // Internal
    private KinematicProjectileLauncher _projectileLauncher;

    private class CrowBrain : MonsterBrain
    {
        public CrowBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged()
                            .AddChild(new Adjusting("Fly"))
                            .AddChild(new DeadEnd()))
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
            AddChild(new MonsterAction(MonsterActionType.Idle));
            AddChild(new MonsterAction("Fly"));
            AddChild(new MonsterAction(MonsterActionType.Hit));
            AddChild(new AttackWithKinematicProjectile(
                monster._projectileLauncher,
                () => new(monster._launchTime, monster._projectileSpeed),
                KinematicProjectileLauncher.LaunchType.Rotation,
                () => new Vector2[] { monster.DetectedPlayer.transform.position - monster.transform.position }
            ));
            AddChild(new MonsterAction(MonsterActionType.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        _projectileLauncher = GetComponent<KinematicProjectileLauncher>();
        _projectileLauncher.Initialize(this, PlatformManager, "Player", "Ground");

        ActionController = new CrowActionController(this);
        ActionController.Enter();

        Brain = new CrowBrain(this);
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
    [CustomEditor(typeof(Crow)), CanEditMultipleObjects]
    private class CrowEditor : MonsterEditor { }
#endif
}
