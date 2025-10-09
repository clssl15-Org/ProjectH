using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FireImp : Monster
{
    // Property
    [Header("Fire Imp")]
    [SerializeField] private GameObject _firePrefab;
    [SerializeField] private float _fireStartTime;


    // Internal
    private class FireImpBrain : MonsterBrain
    {
        public FireImpBrain(Monster owner) : base(owner)
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

    private class FireImpController : MonsterActionController
    {
        public FireImpController(FireImp monster) : base(monster)
        {
            AddChild((object)new MonsterAction(MonsterActionType.Idle)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Run)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Attack)
                .AddComponent(new AttackWithWeapon(monster._firePrefab, monster._fireStartTime)));
            AddChild((object)new MonsterAction(MonsterActionType.Hit)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Dead)
                .AddAnimationComponent());
        }
    }


    // Content
    protected override void Awake()
    {
        if (!_firePrefab)
            throw new InvalidOperationException(
                $"{GetType().Name}은(는) {nameof(_firePrefab)}을(를) 가지고 있어야 합니다.");

        _firePrefab.SetActive(false);
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        ActionController = new FireImpController(this);
        ActionController.Enter();
        
        Brain = new FireImpBrain(this);
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
