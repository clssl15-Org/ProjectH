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
        private Action<KinematicProjectile>[] _initializers;
        private Func<Vector2> _getTargetPoint;
        private Sequence _sequence;

        private KinematicProjectile _spikePrefab;
        private Transform[] _spikeSpawnPositions;
        private SpawnPointType _spawnPointType;
        private KinematicProjectile[] _spikes;


        // Content
        public enum SpawnPointType { World, Local }

        public SpikeAttackAction(
            KinematicProjectile spikePrefab,
            IEnumerable<Transform> spikeSpawnPoints,
            SpawnPointType spawnPointType,
            float projectileSpeed,
            float projectileFireGap)
        {
            _spikeSpawnPositions = spikeSpawnPoints.ToArray();
            _spawnPointType = spawnPointType;

            _spikePrefab = spikePrefab;
            _spikes = new KinematicProjectile[_spikeSpawnPositions.Length];

            _sequence = new Sequence();
            IClip before = null;

            for (int i = 0; i < _spikeSpawnPositions.Length; i++)
            {
                var index = i;
                var clip = new Clip($"SpikeLauncher {i}")
                    .OnStarted((self, _) =>
                    {
                        new SpikeLauncher(
                            _spikes[index],
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

        public SpikeAttackAction SetInitializer(params Action<KinematicProjectile>[] initializers)
        {
            _initializers = initializers;
            return this;
        }

        protected override void OnEnter(object input)
        {
            if (input == null)
                throw new ArgumentNullException(
                    $"{nameof(SpikeAttackAction)}의 입력값은 null일 수 없습니다.");

            if (input is not Func<Vector2> getTargetPoint)
                throw new ArgumentException(
                    $"{nameof(SpikeAttackAction)}의 입력값은 Func<Vector2> 타입이어야 합니다.");

            for (int i = 0; i < _spikes.Length; i++)
            {
                var spike =
                    UnityEngine.Object.Instantiate(_spikePrefab.gameObject)
                    .GetComponent<KinematicProjectile>();

                spike.Initialize(Owner.PlatformManager, "Player", "Ground");

                switch (_spawnPointType)
                {
                    case SpawnPointType.World:
                        spike.transform.SetPositionAndRotation(
                            _spikeSpawnPositions[i].position,
                            _spikeSpawnPositions[i].rotation);
                        break;

                    case SpawnPointType.Local:
                        spike.transform.SetLocalPositionAndRotation(
                            _spikeSpawnPositions[i].position,
                            _spikeSpawnPositions[i].rotation);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(_spawnPointType), _spawnPointType, $"알 수 없는 위치 정보 타입 '{_spawnPointType}'이(가) 입력되었습니다.");
                }

                if (_initializers != null)
                {
                    foreach (var initializer in _initializers)
                        initializer?.Invoke(spike);
                }

                _spikes[i] = spike;
            }

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
            for (int i = 0; i < _spikes.Length; i++)
                _spikes[i] = null;

            _sequence.Stop();
        }
    }
}
