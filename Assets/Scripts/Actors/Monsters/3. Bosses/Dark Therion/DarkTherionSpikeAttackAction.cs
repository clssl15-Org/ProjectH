using System;
using System.Collections.Generic;
using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public partial class DarkTherion
    {
        private class DarkTherionSpikeAttackAction : MonsterActionComponent
        {
            // Internal
            private DarkTherion DarkTherion => (DarkTherion)Owner;

            private sealed class SpikeProjectile
            {
                public Rigidbody2D Body;
                public Vector2 Direction;
                public float StartAngle;
                public float TargetAngle;
                public bool Fired;
            }

            private readonly List<SpikeProjectile> _projectiles = new();

            private float _projectileSpeed;
            private float _aimDuration;

            // 현재 에임/발사를 진행 중인 인덱스
            private int _currentIndex;
            private float _phaseTimer;

            // Content
            public DarkTherionSpikeAttackAction()
            {
                InterruptAllOnDeactivate = true;
            }

            protected override void OnEnter(float _, object input)
            {
                _projectiles.Clear();
                _currentIndex = 0;
                _phaseTimer = 0f;

                if (input == null)
                    throw new ArgumentNullException(
                        $"{nameof(DarkTherionSpikeAttackAction)}의 입력값은 null일 수 없습니다.");

                if (input is not Transform targetTransform)
                    throw new ArgumentException(
                        $"{nameof(DarkTherionSpikeAttackAction)}의 입력값은 Transform 타입이어야 합니다.");

                var stats = DarkTherion.StatsInfo;
                _projectileSpeed = stats.ProjectileSpeed;
                _aimDuration = Mathf.Max(stats.ProjectileFireGap, 0.01f);

                Vector2 targetPos = targetTransform.position;

                var spawnPoints = DarkTherion._spikeSpawnPoints;
                if (spawnPoints == null || spawnPoints.Length == 0)
                    return;

                foreach (var spawn in spawnPoints)
                {
                    if (spawn == null)
                        continue;

                    // 스폰
                    var projectileObject = Instantiate(
                        DarkTherion._spikePrefab.gameObject,
                        spawn.position,
                        Quaternion.identity
                    );

                    projectileObject
                        .GetComponent<SpriteSizeHandler>()
                        .Initialize(DarkTherion._configuration)
                        .RequestApplyScaleFactor();

                    projectileObject
                        .GetComponent<Projectile>()
                        .Initialize(DarkTherion.PlatformManager, "Player", "Ground");

                    if (!projectileObject.TryGetComponent<Rigidbody2D>(out var body))
                        continue;

                    body.velocity = Vector2.zero;
                    body.angularVelocity = 0f;

                    Vector2 origin = spawn.position;
                    Vector2 dir = targetPos - origin;
                    if (dir.sqrMagnitude < 0.0001f)
                        dir = Vector2.down;
                    dir.Normalize();

                    // 스프라이트가 기본적으로 위(+Y)를 보고 있다면 -90도 보정
                    float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                    float startAngle = body.rotation;

                    _projectiles.Add(new SpikeProjectile
                    {
                        Body = body,
                        Direction = dir,
                        StartAngle = startAngle,
                        TargetAngle = targetAngle,
                        Fired = false
                    });
                }
            }

            protected override void OnUpdate(float _)
            {
                if (_projectiles.Count == 0 || _currentIndex >= _projectiles.Count)
                    return;

                var current = _projectiles[_currentIndex];
                if (current.Body == null)
                {
                    // 이미 파괴된 경우 건너뛰기
                    _currentIndex++;
                    _phaseTimer = 0f;
                    return;
                }

                _phaseTimer += Time.deltaTime;

                // 에임 단계: 시작 각도 → 타겟 각도로 보간
                float t = Mathf.Clamp01(_phaseTimer / _aimDuration);
                float angle = Mathf.LerpAngle(current.StartAngle, current.TargetAngle, t * 5f);
                current.Body.MoveRotation(angle);

                // 에임 완료 시 발사
                if (t >= 0.5f && !current.Fired)
                {
                    current.Body.velocity = current.Direction * _projectileSpeed;
                    current.Fired = true;

                    _projectiles[_currentIndex] = current;
                    _currentIndex++;
                    _phaseTimer = 0f;
                }
                else
                {
                    _projectiles[_currentIndex] = current;
                }
            }

            protected override void OnInterrupt(InterruptType _)
            {
                for (int i = 0; i < _projectiles.Count; i++)
                {
                    if (_projectiles[i].Body != null)
                        Destroy(_projectiles[i].Body.gameObject);
                }

                _projectiles.Clear();
            }
        }
    }
}
