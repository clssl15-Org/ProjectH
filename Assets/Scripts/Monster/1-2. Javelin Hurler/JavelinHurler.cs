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
                            .AddChild(new Adjusting(MonsterAction.Walk))
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
            AddChild(new MonsterActionState(MonsterAction.Idle));
            AddChild(new MonsterActionState(MonsterAction.Alert));
            AddChild(new MonsterActionState(MonsterAction.Walk));
            AddChild(new MonsterActionState(MonsterAction.Run));
            AddChild(new JavelinHurlerAttackAction());
            AddChild(new MonsterActionState(MonsterAction.Hit));
            AddChild(new MonsterActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Awake()
    {
        if (!javelinPrefab)
            throw new InvalidOperationException($"창던지개는 {nameof(javelinPrefab)}을(를) 가지고 있어야 합니다.");

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
            new("Hit", new object[] { damageInfo }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
        });
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(JavelinHurler)), CanEditMultipleObjects]
    private class JavelinHurlerEditor : MonsterEditor { }
#endif
}
