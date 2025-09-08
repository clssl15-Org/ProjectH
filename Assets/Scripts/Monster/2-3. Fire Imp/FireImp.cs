using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FireImp : Monster
{
    // Property
    [Header("Fire Imp")]
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private Vector2 firePosition;
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
                        .AddChild(new Engaged(true))
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
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState(MonsterAction.Run));
            AddChild(new AttackWithWeapon(monster.firePrefab, monster.firePosition, monster.fireStartTime));
            AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        ActionController = new FireImpController(this);
        ActionController.Enter();
        
        Brain = new FireImpBrain(this);
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
    [CustomEditor(typeof(FireImp)), CanEditMultipleObjects]
    private class FireImpEditor : MonsterEditor { }
#endif
}
