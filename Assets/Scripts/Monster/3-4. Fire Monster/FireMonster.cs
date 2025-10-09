using System;
using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class FireMonster : Monster
{
    // Property
    [Header("Fire Monster")]
    [SerializeField] private GameObject _firePrefab;
    [SerializeField] private float _fireAppearTime;


    // Internal
    private class FireMonsterBrain : MonsterBrain
    {
        public FireMonsterBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(Engaged.RangeType.Contact, 2f)
                            .AddChild(new Adjusting(MonsterActionType.Idle))
                            .AddChild(new DeadEnd()))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(monsterAction: MonsterActionType.Idle))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class FireMonsterActionController : MonsterActionController
    {
        public FireMonsterActionController(FireMonster monster) : base(monster)
        {
            AddChild(new MonsterAction(MonsterActionType.Idle)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Attack)
                .AddAnimationComponent()
                .AddComponent(new AttackWithWeapon(monster._firePrefab, monster._fireAppearTime)));
            AddChild(new MonsterAction(MonsterActionType.Hit)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Dead)
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

        ActionController = new FireMonsterActionController(this);
        ActionController.Enter();

        Brain = new FireMonsterBrain(this);
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
    [CustomEditor(typeof(FireMonster)), CanEditMultipleObjects]
    private class FireMonsterEditor : MonsterEditor { }
#endif
}
