using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
public partial class StagBeetle : Monster
{
    // Front
    [Header("Stag Beetle")]
    [SerializeField, Min(0)] private float rollingTime = 3f;
    [SerializeField, Min(0)] private float rollingSpeed = 1f;

    // Internal
    public enum AttackMode
    {
        RollAttack,
        SpikeAttack,
        Roar
    }

    private Collider2D colliderComponent;

    private class StagBeetleBrain : MonsterBrain
    {
        public StagBeetleBrain(StagBeetle stagBeetle) : base(stagBeetle)
        {
            AddChild(new Alive()
                .AddChild(new Hit("HitGround"))
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(monsterAction: MonsterAction.Walk))
                        .AddChild(new StagBeetleAttack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol())))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class StagBeetleActionController : MonsterActionController
    {
        public StagBeetleActionController(StagBeetle stagBeetle) : base(stagBeetle)
        {
            bool goRight = default;

            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState(MonsterAction.Alert));
            AddChild(new MonsteActionState(MonsterAction.Walk));
            AddChild(new MonsteActionState(MonsterAction.Run));
            AddChild(new ThreePhasedAction(AttackMode.RollAttack.ToString(),
                n => n + "Anticipation", n => n + "Recoil",
                beforePreAction: () => goRight = stagBeetle.DetectedPlayer.transform.position.x > stagBeetle.transform.position.x,
                beforeMainAction: () => stagBeetle.colliderComponent.excludeLayers = LayerMask.GetMask("Player"),
                whileMainAction: playtime =>
                {
                    if (stagBeetle.TryMove())
                        stagBeetle.Rigidbody.velocity = new Vector2
                        {
                            x = (goRight ? 1 : -1) * stagBeetle.rollingSpeed,
                            y = stagBeetle.Rigidbody.velocity.y,
                        };
                    else
                        stagBeetle.StopMoving();

                    return playtime < stagBeetle.rollingTime;
                },
                afterMainAction: () => stagBeetle.colliderComponent.excludeLayers = default));
            AddChild(new ThreePhasedAction(AttackMode.SpikeAttack.ToString(),
                n => n + "Anticipation", n => n + "Recoil",
                whileMainAction: playtime => playtime < 1)); // TODO: 조건 수정
            AddChild(new ThreePhasedAction(AttackMode.Roar.ToString(),
                n => n + "Anticipation", n => n + "Recoil",
                whileMainAction: playtime => playtime < 1)); // TODO: 조건 수정
            AddChild(new MonsteActionState("HitGround"));
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Awake()
    {
        base.Awake();
        colliderComponent = GetComponent<Collider2D>();
    }

    protected void Start()
    {
        Direction = Random.Range(0, 2) == 0
            ? Direction.Left
            : Direction.Right;

        ActionController = new StagBeetleActionController(this);
        ActionController.Enter();
        
        Brain = new StagBeetleBrain(this);
    }

    protected override void OnDamaged(int damage)
    {
        if (Brain.Blackboard.Committing)
        {
            // TODO: 대미지 효과
            HP -= damage;
        }
        else
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(true),
                new("Hit", new object[] { damage }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
            });
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(StagBeetle)), CanEditMultipleObjects]
    private class StagBeetleEditor : MonsterEditor { }
#endif
}
