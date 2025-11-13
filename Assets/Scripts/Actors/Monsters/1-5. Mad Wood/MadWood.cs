using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters
{
    public partial class MadWood : Monster<MadWoodStats>
    {
        // Internal
        public enum AttackMode
        {
            DefaultAttack,
            LandAttack
        }

        // 이 필드는 MadWoodAttack에서 관리합니다.
        private AttackMode _previousAttackMode = AttackMode.LandAttack;


        private class MadWoodBrain : MonsterBrain
        {
            public MadWoodBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(Engaged.RangeType.Contact)
                                .AddChild(new Adjusting())
                                .AddChild(new DeadEnd()))
                            .AddChild(new MadWoodAttackBrain())
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
            public MadWoodActionController(IMonsterInternal monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction("Fall")
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(AttackMode.DefaultAttack.ToString())
                    .AddAnimationComponent());
                AddChild(new MonsterAction(AttackMode.LandAttack.ToString())
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
            new(nameof(Hit), new object[] { damageInfo }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
            });
        }
    }
}
