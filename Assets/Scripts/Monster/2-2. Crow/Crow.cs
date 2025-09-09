using System;
using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class Crow : Monster
{
    // Property
    [Header("Crow")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField, Min(0)] private float projectileScale = 1;
    [SerializeField] private Vector2 projectilePosition;
    [SerializeField, Min(0)] private float launchTime = 1;
    [SerializeField, Min(0)] private float projectileSpeed;


    // Internal
    private class CrowBrain : MonsterBrain
    {
        public CrowBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(false, "Fly")
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
        public CrowActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState("Fly"));
            AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new MonsteActionState(MonsterAction.Attack));
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Awake()
    {
        //if (!projectilePrefab)
        //    throw new InvalidOperationException($"까마귀는 {nameof(projectilePrefab)}을(를) 가지고 있어야 합니다.");

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

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
