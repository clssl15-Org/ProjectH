using System.Collections.Generic;
using Actors.Monsters.Actions;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public partial class DarkTherion
    {
        private class DarkTherionBulletAttackAction : MonsterActionComponent
        {
            // Internal
            private DarkTherion DarkTherion => (DarkTherion)Owner;

            private sealed class OrbitBullet
            {
                public Rigidbody2D Body;
                public float StartRadius;
                public float RadiusSpeed;
                public float AngularSpeedDeg;
                public float StartAngleDeg;
                public float Elapsed;
            }

            // Content
            protected override void OnEnter(float _, object __)
            {
                var stats = DarkTherion.StatsInfo;
                var bullets = new List<OrbitBullet>();

                int ringCount = stats.BulletCircleCount;
                float initialRadius = stats.BulletCircleStartRadius;
                float spacing = Mathf.Max(stats.BulletCircleSpacing, 0.01f);
                float radiusSpeedBase = stats.BulletCircleRadiusSpeedBase;
                float radiusSpeedStep = stats.BulletCircleRadiusSpeedStep;
                float angularSpeed = stats.BulletAngularSpeed;

                // 모든 링이 공유하는 시작 반지름에서의 둘레 기준으로 탄 개수 산정
                float radiusForSpacing = Mathf.Max(initialRadius, 0.01f);
                float circumference = 2f * Mathf.PI * radiusForSpacing;

                int bulletCount = Mathf.Max(1, Mathf.RoundToInt(circumference / spacing));
                float angleStep = 360f / bulletCount;

                for (int ringIndex = 0; ringIndex < ringCount; ringIndex++)
                {
                    float radiusSpeed = radiusSpeedBase + radiusSpeedStep * ringIndex;

                    // 링 인덱스에 따라 이전 링에서 반 스텝씩 어긋나도록 오프셋
                    float ringAngleOffset = angleStep * 0.5f * ringIndex;

                    for (int i = 0; i < bulletCount; i++)
                    {
                        // 기본 그리드 + 링별 오프셋
                        float startAngle = angleStep * i + ringAngleOffset;

                        var projectileObject = Instantiate(
                            DarkTherion._projectilePrefab.gameObject,
                            DarkTherion.transform.position,
                            Quaternion.identity
                        );

                        projectileObject
                            .GetComponent<Projectile>()
                            .Initialize(DarkTherion.PlatformManager);

                        projectileObject
                            .GetComponent<SpriteSizeHandler>()
                            .Initialize(DarkTherion._configuration)
                            .RequestApplyScaleFactor();

                        if (!projectileObject.TryGetComponent<Rigidbody2D>(out var body))
                            continue;

                        body.bodyType = RigidbodyType2D.Kinematic;
                        body.velocity = Vector2.zero;
                        body.angularVelocity = 0f;

                        bullets.Add(new OrbitBullet
                        {
                            Body = body,
                            StartRadius = initialRadius,
                            RadiusSpeed = radiusSpeed,
                            AngularSpeedDeg = angularSpeed,
                            StartAngleDeg = startAngle,
                            Elapsed = 0f
                        });
                    }
                }

                var centerPos = (Vector2)DarkTherion.transform.position;

                new Timer(
                    DarkTherion._bulletDestroyTime,
                    _ =>
                    {
                        for (int i = 0; i < bullets.Count; i++)
                        {
                            var b = bullets[i];
                            if (b.Body != null)
                                Destroy(b.Body.gameObject);
                        }
                    },
                    () =>
                    {
                        if (bullets.Count == 0)
                            return;

                        for (int i = bullets.Count - 1; i >= 0; i--)
                        {
                            var b = bullets[i];
                            if (b.Body == null)
                            {
                                bullets.RemoveAt(i);
                                continue;
                            }

                            b.Elapsed += Time.deltaTime;

                            float radius = b.StartRadius + b.RadiusSpeed * b.Elapsed;
                            float angleDeg = b.StartAngleDeg + b.AngularSpeedDeg * b.Elapsed;
                            float angleRad = angleDeg * Mathf.Deg2Rad;

                            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
                            Vector2 targetPos = centerPos + offset;

                            b.Body.MovePosition(targetPos);
                            b.Body.MoveRotation(angleDeg + 90f);
                        }
                    });
            }
        }
    }
}
