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
                        .AddChild(new Adjusting(true, MonsterAction.Idle)
                        {
                            UpperRangeTolerance = 4f
                        })
                        .AddChild(new Attack())
                        .AddChild(new Cooldown()))
                    .AddChild(new PlayerNotDetected()
                        .AddChild(new Rest())
                        .AddChild(new Patrol(MonsterAction.Idle))))
                .AddChild(new NotValidPlatform()));
            AddChild(new Dead());
        }
    }

    private class DarkMonsterSecondPhaseController : MonsterActionController
    {
        public DarkMonsterSecondPhaseController(Monster monster) : base(monster)
        {
            AddChild(new MonsteActionState(MonsterAction.Idle));
            AddChild(new MonsteActionState(MonsterAction.Attack));
            //AddChild(new MonsteActionState(MonsterAction.Hit));
            AddChild(new MonsteActionState(MonsterAction.Dead));
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

    protected override void OnDamaged(int damage)
    {
        StandaloneHitBrain.TryTakeDamage(damage);
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(DarkMonsterSecondPhase)), CanEditMultipleObjects]
    private class DarkMonsterSecondPhaseEditor : MonsterEditor { }
#endif
}
