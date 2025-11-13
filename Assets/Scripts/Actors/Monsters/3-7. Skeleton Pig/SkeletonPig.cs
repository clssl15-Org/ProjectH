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
        [SerializeField] private GameObject _shockwave;
        [SerializeField] private float _shockwaveTime;

        public enum AttackMode
        {
            Any,
            DashAttack,
            StampAttack,
            Roar
        }


        // Internal
        private class SkeletonPigBrain : MonsterBrain
        {
            public SkeletonPigBrain(SkeletonPig skeletonPig) : base(skeletonPig)
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
            public SkeletonPigActionController(SkeletonPig skeletonPig) : base(skeletonPig)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Alert)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(AttackMode.DashAttack.ToString())
                    .AddAnimationComponent("Dash"));
                AddChild(new MonsterAction(AttackMode.StampAttack.ToString())
                    .AddAnimationComponent(
                        "Stamp",
                        interruptAllOnDeactivate: true,
                        delayAfterPlay: 0.5f)
                    .AddComponent(new AttackWithWeapon(
                        skeletonPig._shockwave,
                        skeletonPig._shockwaveTime))
                );
                AddChild(new MonsterAction(AttackMode.Roar.ToString())
                    .AddAnimationComponent("Roar"));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }


        // Content
        protected override void Start()
        {
            base.Start();

            ActionController = new SkeletonPigActionController(this);
            ActionController.Enter();

            Brain = new SkeletonPigBrain(this);
            //StandaloneHitBrain.DoKnockback = false;
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
