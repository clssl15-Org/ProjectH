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
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private float fireStartTime;


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
                        .AddChild(new Patrol(MonsterAction.Run))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class FireImpController : MonsterActionController
    {
        public FireImpController(FireImp monster) : base(monster)
        {
            AddChild(new MonsterActionState(MonsterAction.Idle));
            AddChild(new MonsterActionState(MonsterAction.Run));
            AddChild(new AttackWithWeapon(monster.firePrefab, monster.fireStartTime));
            AddChild(new MonsterActionState(MonsterAction.Hit));
            AddChild(new MonsterActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Awake()
    {
        if (!firePrefab)
            throw new InvalidOperationException(
                $"{GetType().Name}은(는) {nameof(firePrefab)}을(를) 가지고 있어야 합니다.");

        firePrefab.SetActive(false);
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
