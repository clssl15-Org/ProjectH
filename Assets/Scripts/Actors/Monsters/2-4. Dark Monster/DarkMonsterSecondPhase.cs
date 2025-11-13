using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public class DarkMonsterSecondPhase : Monster<MonsterStats>
    {
        // Internal
        private class DarkMonsterSecondPhaseBrain : MonsterBrain
        {
            public DarkMonsterSecondPhaseBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    //.AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(Engaged.RangeType.Contact, 1f)
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
            public DarkMonsterSecondPhaseController(IMonsterInternal monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
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
    }
}
