using System;
using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class DarkTherion
    {
        private class DarkTherionProjectileAttackAction : MonsterActionComponent
        {
            private DarkTherion DarkTherion => (DarkTherion)Owner;
            private Action<GameObject>[] _initializers;

            private float _remainingToFire;
            private float _fireCount;
            private Func<Vector2> _getTargetPosition;

            public DarkTherionProjectileAttackAction()
            {
               InterruptPriority = InterruptPriority.High;
            }

            public DarkTherionProjectileAttackAction SetInitializer(params Action<GameObject>[] initializers)
            {
                _initializers = initializers;
                return this;
            }

            protected override void OnEnter(object input)
            {
                if (input == null)
                    throw new ArgumentNullException(
                        $"{nameof(DarkTherionProjectileAttackAction)}의 입력값은 null일 수 없습니다.");

                if (input is not Func<Vector2> getTargetPosition)
                    throw new ArgumentException(
                        $"{nameof(DarkTherionProjectileAttackAction)}의 입력값은 Func<Vector2> 타입이어야 합니다.");

                _remainingToFire = 0f;
                _fireCount = 0;
                _getTargetPosition = getTargetPosition;
            }

            protected override void OnUpdate(float deltaTime)
            {
                if (_fireCount >= DarkTherion.StatsInfo.ProjectileCount)
                {
                    Interrupt(InterruptType.Completed);
                    return;
                }

                var targetPos = _getTargetPosition();

                Owner.Direction = targetPos.x >= Owner.transform.position.x
                    ? Direction.Right
                    : Direction.Left;

                _remainingToFire -= deltaTime;
                if (_remainingToFire <= 0)
                {
                    _remainingToFire = DarkTherion.StatsInfo.ProjectileFireGap;
                    var posDelta = targetPos - (Vector2)DarkTherion._projectileLaunchPoint.position;

                    var projectileGO = Instantiate(DarkTherion._projectilePrefab.gameObject);
                    projectileGO.transform.SetPositionAndRotation(
                        DarkTherion._projectileLaunchPoint.position,
                        KinematicProjectileLauncher.RotationFromDirection(posDelta));

                    if (_initializers != null)
                    {
                        foreach (var initializer in _initializers)
                            initializer?.Invoke(projectileGO);
                    }

                    var projectile = projectileGO.GetComponent<KinematicProjectile>();
                    projectile.Initialize(DarkTherion.PlatformManager);

                    projectile.Launch(
                        posDelta,
                        DarkTherion.StatsInfo.ProjectileSpeed);

                    DarkTherion.AudioPlayer.Play("ProjectileAttack");
                    _fireCount++;
                }
            }
        }
    }
}
