using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class Ghost : Monster
{
    // Property
    [Header("Ghost")]
    [SerializeField] private AttackMode _attackMode = AttackMode.Any;
    [Space()]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float launchStartTime;

    public enum AttackMode
    {
        Any,
        RangedAttack,
        ExplosiveAttack,
    }


    // Internal
    private class GhostBrain : MonsterBrain
    {
        public GhostBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged()
                            .AddChild(new Adjusting(MonsterActionType.Idle))
                            .AddChild(new DeadEnd(MonsterActionType.Idle)))
                        .AddChild(new GhostAttack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(MonsterActionType.Idle))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class GhostController : MonsterActionController
    {
        public GhostController(Ghost monster) : base(monster)
        {
            AddChild((object)new MonsterAction(MonsterActionType.Idle)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Run)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction("Attack_1")
                .AddAnimationComponent());
            AddChild((object)new MonsterAction("Attack_2")
                .AddComponent(new GhostExplosiveAttackAction()));
            AddChild((object)new MonsterAction(MonsterActionType.Hit)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Dead)
                .AddAnimationComponent());
        }
    }


    // Content
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        ActionController = new GhostController(this);
        ActionController.Enter();
        
        Brain = new GhostBrain(this);
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
    [CustomEditor(typeof(FireImp)), CanEditMultipleObjects]
    private class FireImpEditor : MonsterEditor { }
#endif
}
