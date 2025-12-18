using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;
using System;

namespace Actors.Monsters
{
    public partial class MadWood : Monster<MadWoodStats>
    {
        [Header("Mad Wood")]
        [SerializeField] private Weapon _defaultAttackWeapon;
        [SerializeField, Min(0)] private float _defaultAttackActiveTiming;
        [SerializeField] private float _defaultAttackActiveDuration;
        [Space]
        [SerializeField] private Weapon _landAttackWeapon;
        [SerializeField, Min(0)] private float _landAttackActiveTiming;
        [SerializeField] private float _landAttackActiveDuration;


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
            public MadWoodBrain(MadWood owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(
                                    Engaged.RangeType.Ranged,
                                    Mathf.Abs(
                                        owner._landAttackWeapon?.transform.localPosition.x
                                        ?? Engaged.DefaultTargetAttackRange)
                                )
                                .AddChild(new Adjusting())
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new MadWoodAttackBrain())
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol(MonsterActionType.Run))
                        )
                    )
                    .AddChild(new NotValidPlatform("Fall"))
                );
                AddChild(new Dead());
            }
        }

        private class MadWoodActionController : MonsterActionController
        {
            public MadWoodActionController(MadWood monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction("Fall")
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Run)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(AttackMode.DefaultAttack.ToString())
                    .AddAnimationComponent(interruptAllOnDeactivate: true)
                    .AddComponent(new AttackWithWeapon(
                        monster._defaultAttackWeapon,
                        monster._defaultAttackActiveTiming,
                        monster._defaultAttackActiveDuration))
                );
                AddChild(new MonsterAction(AttackMode.LandAttack.ToString())
                    .AddAnimationComponent(interruptAllOnDeactivate: true)
                    .AddComponent(new AttackWithWeapon(
                        monster._landAttackWeapon,
                        monster._landAttackActiveTiming,
                        monster._landAttackActiveDuration))
                );
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent()
                );
            }
        }


        // Content
        protected override void Start()
        {
            base.Start();

            if (_defaultAttackWeapon)
                _defaultAttackWeapon.AttackPower = StatsInfo.AttackPower;
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_defaultAttackWeapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            if (_landAttackWeapon)
                _landAttackWeapon.AttackPower = StatsInfo.GroundAttackPower;
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_landAttackWeapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

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
