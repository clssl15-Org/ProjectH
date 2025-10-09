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
    private bool UseLandAttackOverride => UseStatsOverride && Stats?.Length >= 2 && Stats[1];

    public override int AttackPower
    {
        get
        {
            if (previousAttackMode == AttackMode.DefaultAttack)
                return base.AttackPower;

            return !UseLandAttackOverride ? _landAttackPower : Stats[1].AttackPower;
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
                        .AddChild(new Engaged(Engaged.RangeType.Contact)
                            .AddChild(new Adjusting())
                            .AddChild(new DeadEnd()))
                        .AddChild(new MadWoodAttack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(MonsterActionType.Run))))
                .AddChild(new NotValidPlatform("Fall")));
            AddChild(new Dead());
        }
    }

    private class MadWoodActionController : MonsterActionController
    {
        public MadWoodActionController(Monster monster) : base(monster)
        {
            AddChild((object)new MonsterAction(MonsterActionType.Idle)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction("Fall")
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Run)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(AttackMode.DefaultAttack.ToString())
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(AttackMode.LandAttack.ToString())
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Hit)
                .AddAnimationComponent());
            AddChild((object)new MonsterAction(MonsterActionType.Dead)
                .AddAnimationComponent());
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
