using System;
using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class JavelinHurler : Monster
{
    // Property
    [Header("Javelin Hurler")]
    [SerializeField] private GameObject javelinPrefab;
    [SerializeField, Min(0)] private float javelinScale = 1;
    [SerializeField] private Vector2 javelinPosition;
    [SerializeField, Min(0)] private float throwTime = 1;
    [SerializeField, Range(0, 90)] private float throwAngle;
    [SerializeField, Min(0)] private float throwPower;


    // Internal
    private class JavelinHurlerBrain : MonsterBrain
    {
        public JavelinHurlerBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(range: 5f)
                            .AddChild(new Adjusting(MonsterActionType.Walk))
                            .AddChild(new DeadEnd()))
                        .AddChild(new Attack()))
                        // 창던지개는 Cooldown을 가지지 않습니다.
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol())))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class JavelinHurlerActionController : MonsterActionController
    {
        public JavelinHurlerActionController(Monster monster) : base(monster)
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
                .AddAnimationComponent()
                .AddComponent(new HurlJavelin()));
            AddChild(new MonsterAction(MonsterActionType.Hit)
                .AddAnimationComponent());
            AddChild(new MonsterAction(MonsterActionType.Dead)
                .AddAnimationComponent());
        }
    }


    // Content
    protected override void Awake()
    {
        if (!javelinPrefab)
            throw new InvalidOperationException($"{nameof(JavelinHurler)}은(는) {nameof(javelinPrefab)}을(를) 가지고 있어야 합니다.");

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        ActionController = new JavelinHurlerActionController(this);
        ActionController.Enter();

        Brain = new JavelinHurlerBrain(this);
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


#if UNITY_EDITOR
    [CustomEditor(typeof(JavelinHurler)), CanEditMultipleObjects]
    private class JavelinHurlerEditor : MonsterEditor { }
#endif
}
