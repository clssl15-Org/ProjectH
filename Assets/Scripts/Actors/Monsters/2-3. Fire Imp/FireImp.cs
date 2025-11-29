using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public class FireImp : Monster<MonsterStats>
    {
        // Property
        [Header("Fire Imp")]
        [SerializeField] private GameObject _firePrefab;
        [SerializeField] private float _fireStartTime;


        // Internal
        private class FireImpBrain : MonsterBrain
        {
            public FireImpBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(Engaged.RangeType.Contact, 2f)
                                .AddChild(new Adjusting())
                                .AddChild(new DeadEnd()))
                            .AddChild(new Attack())
                            .AddChild(new Cooldown()))
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol(MonsterActionType.Run))))
                    .AddChild(new NotValidPlatform()));
                AddChild(new Dead());
            }
        }

        private class FireImpController : MonsterActionController
        {
            public FireImpController(FireImp monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(interruptAllOnDeactivate: true)
                    .AddComponent(new AttackWithWeapon(monster._firePrefab, monster._fireStartTime)));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }


        // Content
        protected override void Awake()
        {
            if (!_firePrefab)
                throw new InvalidOperationException(
                    $"{GetType().Name}은(는) {nameof(_firePrefab)}을(를) 가지고 있어야 합니다.");

            _firePrefab.SetActive(false);
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new FireImpController(this);
            ActionController.Enter();

            Brain = new FireImpBrain(this);
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
