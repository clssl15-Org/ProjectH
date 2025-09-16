using MonsterActions;
using MonsterBT;
using UniEngine.StateMachines.BT;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class MadWood : Monster
{
    // Property
    [Header("Mad Wood")]
    [SerializeField, Min(0)] private int _landAttackPower = 1;

    // Control
    private bool UseLandAttackOverride => UseStatsOverride && stats?.Length >= 2 && stats[1];

    public override int AttackPower
    {
        get
        {
            if (previousAttackMode == AttackMode.DefaultAttack)
                return base.AttackPower;

            return !UseLandAttackOverride ? _landAttackPower : stats[1].AttackPower;
        }
    }

    // Internal
    public enum AttackMode
    {
        DefaultAttack,
        LandAttack
    }

    // 이 필드는 MadWoodAttack에서 관리합니다.
    private AttackMode previousAttackMode = AttackMode.LandAttack;


    private class MadWoodBrain : MonsterBrain
    {
        public MadWoodBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                .AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Adjusting(true))
                        .AddChild(new MadWoodAttack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(MonsterAction.Run))))
                .AddChild(new NotValidPlatform("Fall")));
            AddChild(new Dead());
        }
    }

    private class MadWoodActionController : MonsterActionController
    {
        public MadWoodActionController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState("Fall"));
            AddChild(new MonsteActionState(MonsterAction.Run));
            AddChild(new MonsteActionState(AttackMode.DefaultAttack.ToString()));
            AddChild(new MonsteActionState(AttackMode.LandAttack.ToString()));
            AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new MonsteActionState(MonsterAction.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        ActionController = new MadWoodActionController(this);
        ActionController.Enter();
        
        Brain = new MadWoodBrain(this);

        // 시작 시 Fall 방지
        // TODO: 추락 State 추가할 것 (플랫폼 미확인과 추락 분리)
        Brain.Tick();
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
    [CustomEditor(typeof(MadWood)), CanEditMultipleObjects]
    private class MadWoodEditor : MonsterEditor
    {
        protected override string[] GetHidingFields()
        {
            if (!((MadWood)target).UseLandAttackOverride)
                return base.GetHidingFields();
            else
                return base.GetHidingFields().Append("_landAttackPower").ToArray();
        }
    }
#endif
}
