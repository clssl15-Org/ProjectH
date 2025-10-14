using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;

public class DarkMonsterFirstPhase : Monster<MonsterStats>
{
    // Property
    [Header("Dark Monster First Phase")]
    [SerializeField] private bool _revive = true;
    [SerializeField] private GameObject _secondPhasePrefab;


    // Internal
    private class DarkMonsterFirstPhaseBrain : MonsterBrain
    {
        public DarkMonsterFirstPhaseBrain(IMonster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(Engaged.RangeType.Contact)
                            .AddChild(new Adjusting(MonsterActionType.Walk))
                            .AddChild(new DeadEnd()))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol())))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class DarkMonsterFirstPhaseController : MonsterActionController
    {
        public DarkMonsterFirstPhaseController(IMonster monster) : base(monster)
        {
            AddChild(new MonsterAction(MonsterActionType.Idle)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Walk)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Attack)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Hit)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Dead)
                .AddAnimationComponent());
        }
    }


    // Content
    protected override void Awake()
    {
        if (_revive && !_secondPhasePrefab)
            Debug.LogWarning(FormatLogMessage(
                $"{nameof(_secondPhasePrefab)}이(가) 유효하지 않기 때문에 사망 후 두 번째 페이즈의 몬스터가 생성되지 않습니다."));

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        ActionController = new DarkMonsterFirstPhaseController(this);
        ActionController.Enter();
        
        Brain = new DarkMonsterFirstPhaseBrain(this);

        Died += succeeded =>
        {
            if (succeeded && _revive && _secondPhasePrefab)
            {
                var second = Instantiate(_secondPhasePrefab);
                second.GetComponent<DarkMonsterSecondPhase>().Initialize(PlatformManager, SceneAssetsLibrary);

                second.transform.position = transform.position;
            }
        };
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
