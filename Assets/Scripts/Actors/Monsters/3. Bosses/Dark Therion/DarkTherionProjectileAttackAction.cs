using System;
using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public partial class DarkTherion
    {
        private class DarkTherionProjectileAttackAction : MonsterActionComponent
        {
            private DarkTherion DarkTherion => (DarkTherion)Owner;

            private float _lastFireTime;
            private float _fireCount;
            private Transform _targetPosition;

            public DarkTherionProjectileAttackAction()
            {
                InterruptAllOnDeactivate = true;
            }

            protected override void OnEnter(float startTime, object input)
            {
                if (input == null)
                    throw new ArgumentNullException(
                        $"{nameof(DarkTherionProjectileAttackAction)}의 입력값은 null일 수 없습니다.");

                if (input is not Transform targetPosition)
                    throw new ArgumentException(
                        $"{nameof(DarkTherionProjectileAttackAction)}의 입력값은 Transform 타입이어야 합니다.");

                _lastFireTime = int.MinValue;
                _fireCount = 0;
                _targetPosition = targetPosition;
            }

            protected override void OnUpdate(float elapsedTime)
            {
                if (_fireCount >= DarkTherion.StatsInfo.ProjectileCount)
                {
                    Interrupt(InterruptType.Completed);
                    return;
                }

                if (elapsedTime - _lastFireTime >= DarkTherion.StatsInfo.ProjectileFireGap)
                {
                    _lastFireTime = elapsedTime;
                    var posDelta = _targetPosition.transform.position - DarkTherion.transform.position;

                    var projectileGO = Instantiate(DarkTherion._projectilePrefab.gameObject);
                    projectileGO.transform.SetPositionAndRotation(
                        DarkTherion.transform.position,
                        KinematicProjectileLauncher.RotationFromDirection(posDelta));

                    if (projectileGO.TryGetComponent<SpriteSizeHandler>(out var ssh))
                    {
                        ssh.Initialize(DarkTherion._configuration);
                        ssh.RequestApplyScaleFactor();
                    }

                    var projectile = projectileGO.GetComponent<KinematicProjectile>();
                    projectile.Initialize(DarkTherion.PlatformManager, "Player", "Ground");

                    projectile.Launch(
                        posDelta,
                        DarkTherion.StatsInfo.ProjectileSpeed);

                    _fireCount++;
                }
            }
        }
    }
}
