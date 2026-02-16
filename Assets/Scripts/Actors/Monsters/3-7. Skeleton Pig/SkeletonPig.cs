using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class SkeletonPig : Monster<SkeletonPigStats>
    {
        // Front
        [Header("Skeleton Pig")]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [Space]
        [SerializeField] private Weapon _dashWeapon;
        [SerializeField, Min(0)] private float _dashWeaponActiveTiming;
        [SerializeField] private float _dashWeaponActiveDuration;
        [Space]
        [SerializeField] private WeaponManager _shockwave;
        [SerializeField] private float _shockwaveTime;
        [Space]
        [Header("Animation")]
        [SerializeField, Min(0)] private float _dieTimeScale = 1f;

        public enum AttackMode
        {
            Any,
            DashAttack,
            StampAttack,
            Roar
        }


        // States
        private class SkeletonPigBrain : MonsterBrain
        {
            public SkeletonPigBrain(SkeletonPig owner) : base(owner)
            {
                AddChild(new Alive()
                    //.AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged()
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new SkeletonPigAttackBrain())
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol())
                        )
                    )
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class SkeletonPigActionController : MonsterActionController
        {
            public SkeletonPigActionController(SkeletonPig monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Alert)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(AttackMode.DashAttack.ToString())
                    .AddAnimationComponent(
                        "Dash",
                        interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(
                        monster._dashWeapon,
                        monster._dashWeaponActiveTiming,
                        monster._dashWeaponActiveDuration))
                );
                AddChild(new MonsterAction(AttackMode.StampAttack.ToString())
                    .AddAnimationComponent(
                        "Stamp",
                        interruptPriority: InterruptPriority.High,
                        delayAfterPlay: 0.5f)
                    .AddComponent(new AttackWithWeapon(
                        monster._shockwave,
                        monster._shockwaveTime,
                        onInstantiate: weapon => weapon.SetKnockbackInfo(() =>
                        {
                            if (!monster) return null;
                            return monster.Direction;
                        }))
                    )
                );
                AddChild(new MonsterAction(AttackMode.Roar.ToString())
                    .AddAnimationComponent("Roar")
                );
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddComponent(new SetTimeScale(monster._dieTimeScale))
                    .AddAnimationComponent()
                );
            }
        }


        // Content
        protected override void Awake()
        {
            if (!_shockwave)
                throw new InvalidOperationException(
                    $"{nameof(SkeletonPig)}은(는) {nameof(_shockwave)}을(를) 가지고 있어야 합니다.");

            _shockwave.AttackPower = StatsInfo.AttackPower;
            _shockwave.gameObject.SetActive(false);

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if (_dashWeapon)
            {
                _dashWeapon.AttackPower = StatsInfo.AttackPower;
                _dashWeapon.gameObject.SetActive(false);
            }
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_dashWeapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            ActionController = new SkeletonPigActionController(this);
            ActionController.Enter();

            Brain = new SkeletonPigBrain(this);
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            StandaloneHitBrain.TryTakeDamage(new DamageInfo
            {
                Damage = damageInfo.Damage,
                HasKnockback = !Brain.Blackboard.Committing && damageInfo.HasKnockback,
                Direction = damageInfo.Direction,
                KnockbackForce = damageInfo.KnockbackForce,
            });
        }
    }
}
