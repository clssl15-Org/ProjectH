using System;
using System.Collections.Generic;
using System.Linq;
using Actors.Monsters.Actions;
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
        private float _elapsedTime;


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
            var clips = new IClip[points.Length];

            _sequence = new Sequence();

            for (int i = 0; i < points.Length; i++)
            {
                var index = i;
                var clip = new Clip($"SpikeLauncher {i}")
                    .AssignTo(out var self)
                    .OnStarted((_, _) =>
                    {
                        var spike =
                            UnityEngine.Object.Instantiate(spikePrefab.gameObject)
                            .GetComponent<KinematicProjectile>();
                        spike.Initialize(Owner.PlatformManager, "Player", "Ground");

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

                clips[i] = clip;

                if (i == 0)
                    _sequence.Add(clip);
                else
                {
                    _sequence.AddAfter(
                        clips[i - 1],
                        new Scp.Delay($"Delay {i}", projectileFireGap)
                            .AssignTo(out var delay)
                    );

                    _sequence.AddAfter(delay, clip);
                }
            }
        }

        protected override void OnEnter(float elapsedTime, object input)
        {
            if (input == null)
                throw new ArgumentNullException(
                    $"{nameof(SpikeAttackAction)}의 입력값은 null일 수 없습니다.");

            if (input is not Func<Vector2> getTargetPoint)
                throw new ArgumentException(
                    $"{nameof(SpikeAttackAction)}의 입력값은 Func<Vector2> 타입이어야 합니다.");

            _getTargetPoint = getTargetPoint;
            _elapsedTime = elapsedTime;
            _sequence.Start();
        }

        protected override void OnUpdate(float elapsedTime)
        {
            var deltaTime = elapsedTime - _elapsedTime;
            _elapsedTime = elapsedTime;

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
