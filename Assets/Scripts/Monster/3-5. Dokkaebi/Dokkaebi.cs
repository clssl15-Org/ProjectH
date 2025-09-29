using System;
using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class Dokkaebi : Monster
{
    // Property
    [Header("Dokkaebi")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private float laserAppearTime;


    // Internal
    private class DokkaebiBrain : MonsterBrain
    {
        public DokkaebiBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged()
                            .AddChild(new Adjusting(MonsterAction.Idle))
                            .AddChild(new DeadEnd()))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(monsterAction: MonsterAction.Idle))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class DokkaebiActionController : MonsterActionController
    {
        public DokkaebiActionController(Dokkaebi monster) : base(monster)
        {
            AddChild(new MonsterActionState(MonsterAction.Idle.ToString()));
            AddChild(new MonsterActionState(MonsterAction.Alert.ToString()));
            AddChild(new MonsterActionState(MonsterAction.Walk.ToString()));
            AddChild(new MonsterActionState(MonsterAction.Run.ToString()));
            AddChild(new AttackWithWeapon(monster.laserPrefab, monster.laserAppearTime));
            AddChild(new MonsterActionState(MonsterAction.Hit.ToString()));
            AddChild(new MonsterActionState(MonsterAction.Dead.ToString()));
        }
    }


    // Content
    protected override void Awake()
    {
        if (!laserPrefab)
            throw new InvalidOperationException(
                $"{GetType().Name}은(는) {nameof(laserPrefab)}을(를) 가지고 있어야 합니다.");

        laserPrefab.SetActive(false);
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        ActionController = new DokkaebiActionController(this);
        ActionController.Enter();

        Brain = new DokkaebiBrain(this);
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
    [CustomEditor(typeof(Dokkaebi)), CanEditMultipleObjects]
    private class DokkaebiEditor : MonsterEditor { }
#endif
}
