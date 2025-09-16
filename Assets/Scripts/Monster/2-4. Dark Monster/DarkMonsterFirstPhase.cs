using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DarkMonsterFirstPhase : Monster
{
    // Property
    [Header("Dark Monster First Phase")]
    [SerializeField] private GameObject secondPhasePrefab;


    // Internal
    private class DarkMonsterFirstPhaseBrain : MonsterBrain
    {
        public DarkMonsterFirstPhaseBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Adjusting(true, MonsterAction.Walk))
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
        public DarkMonsterFirstPhaseController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState(MonsterAction.Walk));
            AddChild(new MonsteActionState(MonsterAction.Attack));
            AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Awake()
    {
        if (!secondPhasePrefab)
            Debug.LogWarning(
                Ctx($"{nameof(secondPhasePrefab)}이(가) 유효하지 않기 때문에 사망 후 두 번째 페이즈의 몬스터가 생성되지 않습니다."));

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
            if (succeeded && secondPhasePrefab)
            {
                var second = Instantiate(secondPhasePrefab);
                second.GetComponent<DarkMonsterSecondPhase>().Initialize(platformManager, sceneAssetsLibrary);

                second.transform.position = transform.position;
            }
        };
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
    [CustomEditor(typeof(DarkMonsterFirstPhase)), CanEditMultipleObjects]
    private class DarkMonsterFirstPhaseEditor : MonsterEditor { }
#endif
}
