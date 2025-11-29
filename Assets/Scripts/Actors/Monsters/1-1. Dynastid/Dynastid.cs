using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters
{
    public class Dynastid : Monster<MonsterStats>
    {
        // Internal
        private class DynastidBrain : MonsterBrain
        {
            public DynastidBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(Engaged.RangeType.Contact, 1)
                                .AddChild(new Adjusting())
                                .AddChild(new DeadEnd()))
                            .AddChild(new Attack())
                            .AddChild(new Cooldown()))
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol())))
                    .AddChild(new NotValidPlatform()));
                AddChild(new Dead());
            }
        }

        private class DynastidActionController : MonsterActionController
        {
            public DynastidActionController(IMonsterInternal monster) : base(monster)
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
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }


        // Content
        protected override void Start()
        {
            base.Start();

            ActionController = new DynastidActionController(this);
            ActionController.Enter();

            Brain = new DynastidBrain(this);
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
    }
}
