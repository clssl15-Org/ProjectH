using MonsterActions;
using MonsterBT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(StandaloneHitAction))]
public class DarkMonsterSecondPhase : Monster
{
    // Internal
    private class DarkMonsterSecondPhaseBrain : MonsterBrain
    {
        public DarkMonsterSecondPhaseBrain(Monster owner) : base(owner)
        {
            AddChild(new Alive()
                //.AddChild(new Hit())
                .AddChild(new ValidPlatform()
                    .AddChild(new PlayerDetected()
                        .AddChild(new Engaged(Engaged.RangeType.Contact, 4f)
                            .AddChild(new Adjusting(MonsterActionType.Idle))
                            .AddChild(new DeadEnd()))
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(MonsterActionType.Idle))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class DarkMonsterSecondPhaseController : MonsterActionController
    {
        public DarkMonsterSecondPhaseController(Monster monster) : base(monster)
        {
            AddChild(new MonsterAction(MonsterActionType.Idle));
            AddChild(new MonsterAction(MonsterActionType.Attack));
            //AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new MonsterAction(MonsterActionType.Dead));
        }
    }


    // Content
    protected override void Start()
    {
        base.Start();

        ActionController = new DarkMonsterSecondPhaseController(this);
        ActionController.Enter();
        
        Brain = new DarkMonsterSecondPhaseBrain(this);
    }

    protected override void OnDamaged(DamageInfo damageInfo)
    {
        StandaloneHitBrain.TryTakeDamage(damageInfo);
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(DarkMonsterSecondPhase)), CanEditMultipleObjects]
    private class DarkMonsterSecondPhaseEditor : MonsterEditor { }
#endif
}
