using System;
using System.Collections.Generic;
using System.Linq;
using Actors.Monsters.Actions;
using Infrastructure;
using Infrastructure.StateMachines.Scp;
using UnityEngine;
using Scp = Infrastructure.StateMachines.Scp;

namespace Actors.Monsters.Bosses
{
    internal class SpikeAttackAction : MonsterActionComponent
    {
        // Internal
        private Func<Vector2> _getTargetPoint;
        private Sequence _sequence;


        // Content
        public enum SpawnPointType { World, Local }

        public SpikeAttackAction(
            KinematicProjectile spikePrefab,
            IEnumerable<Vector2> spikeSpawnPoints,
            SpawnPointType spawnPointType,
            float projectileSpeed,
            float projectileFireGap)
        {
            var points = spikeSpawnPoints.ToArray();

            _sequence = new Sequence();
            IClip before = null;

            for (int i = 0; i < points.Length; i++)
            {
                var index = i;
                var clip = new Clip($"SpikeLauncher {i}")
                    .OnStarted((self, _) =>
                    {
                        var spike =
                            UnityEngine.Object.Instantiate(spikePrefab.gameObject)
                            .GetComponent<KinematicProjectile>();
                        spike.Initialize(Owner.PlatformManager, "Player", "Ground");

                        if (spike.TryGetComponent<SpriteSizeHandler>(out var ssh))
                            ssh.Initialize(Owner.Configuration, true);

                        spike.transform.position = spawnPointType switch
                        {
                            SpawnPointType.World => points[index],
                            SpawnPointType.Local => Owner.transform.TransformPoint(points[index]),
                            _ => throw new ArgumentOutOfRangeException(
                                nameof(spawnPointType), spawnPointType, $"알 수 없는 위치 정보 타입 '{spawnPointType}'이(가) 입력되었습니다."),
                        };

                        new SpikeLauncher(
                            spike,
                            _getTargetPoint,
                            projectileSpeed)
                        .Fire();

                        self.Stop();
                    });

                if (i == 0)
                {
                    before = clip;
                    _sequence.Add(clip);
                }
                else
                {
                    _sequence.AddAfter(
                        before,
                        new Scp.Delay($"Delay {i}", projectileFireGap)
                            .AssignTo(out var delay)
                    );

                    before = delay;
                    _sequence.AddAfter(delay, clip);
                }
            }
        }

        protected override void OnEnter(object input)
        {
            if (input == null)
                throw new ArgumentNullException(
                    $"{nameof(SpikeAttackAction)}의 입력값은 null일 수 없습니다.");

            if (input is not Func<Vector2> getTargetPoint)
                throw new ArgumentException(
                    $"{nameof(SpikeAttackAction)}의 입력값은 Func<Vector2> 타입이어야 합니다.");

            _getTargetPoint = getTargetPoint;
            _sequence.Start();
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!_sequence.Update(deltaTime, out var succeeded))
            {
                Interrupt(succeeded
                    ? InterruptType.Completed
                    : InterruptType.Interrupted);
            }
        }

        protected override void OnInterrupt(InterruptType _)
        {
            _sequence.Stop();
        }
    }
}
