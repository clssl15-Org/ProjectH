using System;
using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;

public class Dokkaebi : Monster<MonsterStats>
{
    // Property
    [Header("Dokkaebi")]
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private float _laserAppearTime;


    // Internal
    private class DokkaebiBrain : MonsterBrain
    {
        public DokkaebiBrain(IMonster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged()
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

    private class DokkaebiActionController : MonsterActionController
    {
        public DokkaebiActionController(Dokkaebi monster) : base(monster)
        {
            AddChild(new MonsterAction(MonsterActionType.Idle)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Alert)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Walk)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Run)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Attack)
                .AddAnimationComponent(interruptAllOnDeactivate: true)
                .AddComponent(new AttackWithWeapon(monster._laserPrefab, monster._laserAppearTime)));
            AddChild(new MonsterAction(MonsterActionType.Hit)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Dead)
                .AddAnimationComponent());
        }
    }


    // Content
    protected override void Awake()
    {
        if (!_laserPrefab)
            throw new InvalidOperationException(
                $"{nameof(Dokkaebi)}은(는) {nameof(_laserPrefab)}을(를) 가지고 있어야 합니다.");

        _laserPrefab.SetActive(false);
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
            new(nameof(Hit), new object[] { damageInfo }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
        });
    }
}
