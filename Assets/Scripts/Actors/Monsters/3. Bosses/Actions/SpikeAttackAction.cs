using System;
using System.Collections.Generic;
using System.Linq;
using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    internal class SpikeAttackAction : MonsterActionComponent
    {
        // Internal
        private sealed class SpikeProjectile
        {
            public Rigidbody2D Body;
            public Vector2 Direction
            {
                get
                {
                    var (origin, dir) = _getLaunchInfo();
                    if (dir.sqrMagnitude < 0.0001f)
                        dir = Vector2.down;
                    dir.Normalize();

                    return dir;
                }
            }
            public float StartAngle;
            public float TargetAngle
            {
                get
                {
                    var (origin, dir) = _getLaunchInfo();
                    if (dir.sqrMagnitude < 0.0001f)
                        dir = Vector2.down;
                    dir.Normalize();

                    return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                }
            }
            public bool Fired;

            private Func<(Vector2 origin, Vector2 dir)> _getLaunchInfo;

            public SpikeProjectile(Func<(Vector2 origin, Vector2 dir)> getLaunchInfo) =>
                _getLaunchInfo = getLaunchInfo;
        }

        private readonly List<SpikeProjectile> _projectiles = new();

        private GameObject _spikePrefab;
        private Vector2[] _spikeSpawnPoints;
        private float _projectileSpeed;
        private float _projectileFireGap;

        private int _currentIndex;
        private float _phaseTimer;

        // Content
        public SpikeAttackAction(
            Projectile spikePrefab,
            IEnumerable<Vector2> spikeSpawnPoints,
            float projectileSpeed,
            float projectileFireGap)
        {
            _spikePrefab = spikePrefab.gameObject;
            _projectileSpeed = projectileSpeed;
            _spikeSpawnPoints = spikeSpawnPoints.ToArray();
            _projectileFireGap = projectileFireGap;

            InterruptAllOnDeactivate = true;
        }

        protected override void OnEnter(float _, object input)
        {
            _projectiles.Clear();
            _currentIndex = 0;
            _phaseTimer = 0f;

            if (input == null)
                throw new ArgumentNullException(
                    $"{nameof(SpikeAttackAction)}의 입력값은 null일 수 없습니다.");

            if (input is not Func<Vector2> getTargetPoint)
                throw new ArgumentException(
                    $"{nameof(SpikeAttackAction)}의 입력값은 Func<Vector2> 타입이어야 합니다.");

            if (_spikeSpawnPoints == null || _spikeSpawnPoints.Length == 0)
                return;

            foreach (var spawnPoint in _spikeSpawnPoints)
            {
                var currentPoint = spawnPoint + (Vector2)Owner.transform.position;

                var projectileObject = UnityEngine.Object.Instantiate(
                    _spikePrefab,
                    currentPoint,
                    Quaternion.identity);

                projectileObject
                    .GetComponent<SpriteSizeHandler>()
                    .Initialize(Owner.Configuration)
                    .RequestApplyScaleFactor();

                projectileObject
                    .GetComponent<Projectile>()
                    .Initialize(Owner.PlatformManager, "Player", "Ground");

                if (!projectileObject.TryGetComponent<Rigidbody2D>(out var body))
                    continue;

                body.velocity = Vector2.zero;
                body.angularVelocity = 0f;
                var startAngle = body.rotation;

                _projectiles.Add(new SpikeProjectile(() => (currentPoint, getTargetPoint() - currentPoint))
                {
                    Body = body,
                    StartAngle = startAngle,
                    Fired = false
                });
            }
        }

        protected override void OnUpdate(float _)
        {
            if (_projectiles.Count == 0 || _currentIndex >= _projectiles.Count)
                return;

            var current = _projectiles[_currentIndex];
            if (!current.Body)
            {
                // 이미 파괴된 경우 건너뛰기
                _currentIndex++;
                _phaseTimer = 0f;
                return;
            }

            _phaseTimer += Time.deltaTime;

            // 에임 단계: 시작 각도를 타겟 각도로 보간
            var t = Mathf.Clamp01(_phaseTimer / _projectileFireGap);
            var angle = Mathf.LerpAngle(current.StartAngle, current.TargetAngle, t * 5f);
            current.Body.MoveRotation(angle);

            // 에임 완료 시 발사
            if (t >= 1f && !current.Fired)
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
                    UnityEngine.Object.Destroy(_projectiles[i].Body.gameObject);
            }

            _projectiles.Clear();
        }
    }
}
