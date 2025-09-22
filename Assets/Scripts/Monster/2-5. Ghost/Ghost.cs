using System;
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
                            .AddChild(new Adjusting(MonsterAction.Idle))
                            .AddChild(new DeadEnd(MonsterAction.Idle)))
                        .AddChild(new GhostAttack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(MonsterAction.Idle))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class GhostController : MonsterActionController
    {
        public GhostController(Ghost monster) : base(monster)
        {
            AddChild(new MonsterActionState(MonsterAction.Idle));
            AddChild(new MonsterActionState(MonsterAction.Run));
            AddChild(new MonsterActionState("Attack_1"));
            //AddChild(new AttackWithWeapon(monster.projectilePrefab, monster.launchStartTime));
            AddChild(new GhostExplosiveAttackAction());
            AddChild(new MonsterActionState(MonsterAction.Hit));
            AddChild(new MonsterActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Awake()
    {
        // TODO: 투사체 발사 추가
        //if (!projectilePrefab)
        //    throw new InvalidOperationException(
        //        $"{GetType().Name}은(는) {nameof(projectilePrefab)}을(를) 가지고 있어야 합니다.");

        //projectilePrefab.SetActive(false);
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        ActionController = new GhostController(this);
        ActionController.Enter();
        
        Brain = new GhostBrain(this);
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
    [CustomEditor(typeof(FireImp)), CanEditMultipleObjects]
    private class FireImpEditor : MonsterEditor { }
#endif
}
