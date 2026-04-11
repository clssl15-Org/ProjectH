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
        private Action<KinematicProjectile>[] _initializers;
        private Func<Vector2> _getTargetPoint;
        private Sequence _sequence;

        private KinematicProjectile _spikePrefab;
        private Transform[] _spikeSpawnPositions;
        private SpawnPointType _spawnPointType;
        private KinematicProjectile[] _spikes;
        private bool _standalone;

        private IDisposable _standaloneUpdateHandle;

        // Content
        public enum SpawnPointType { World, Local }

        public SpikeAttackAction(
            KinematicProjectile spikePrefab,
            IEnumerable<Transform> spikeSpawnPoints,
            SpawnPointType spawnPointType,
            float projectileSpeed,
            float projectileFireGap,
            bool standalone = false)
        {
            _spikeSpawnPositions = spikeSpawnPoints.ToArray();
            _spawnPointType = spawnPointType;
            _standalone = standalone;

            _spikePrefab = spikePrefab;
            _spikes = new KinematicProjectile[_spikeSpawnPositions.Length];

            _sequence = new Sequence();
            IClip before = null;

            bool errorOccurred = false;
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

                        try
                        {
                            ((IBoss)Owner).AudioPlayer.Play("SpikeAttack");
                        }
                        catch
                        {
                            errorOccurred = true;
                        }

                        self.Stop();
                    });

                // 씬 전환 등
                if (errorOccurred) break;

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

            if (_standalone)
                _standaloneUpdateHandle = Loco.Subscribe(StandaloneUpdate);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_standalone) return;

            if (!_sequence.Update(deltaTime, out var succeeded))
            {
                Interrupt(succeeded
                    ? InterruptType.Completed
                    : InterruptType.Interrupted);

                for (int i = 0; i < _spikes.Length; i++)
                    _spikes[i] = null;
            }
        }

        private void StandaloneUpdate()
        {
            if (!_sequence.Update(Time.deltaTime, out var succeeded))
            {
                // 스탠드얼론 모드에서 시퀀스가 끝까지 실행(발사) 완료되면
                // 여기서 자체적으로 루프 핸들을 해제하여 고아(Orphan) 루프를 방지합니다.
                _standaloneUpdateHandle?.Dispose();
                _standaloneUpdateHandle = null;

                Interrupt(succeeded
                    ? InterruptType.Completed
                    : InterruptType.Interrupted);

                for (int i = 0; i < _spikes.Length; i++)
                    _spikes[i] = null;
            }
        }

        protected override void OnInterrupt(InterruptType _)
        {
            // 일반 모드(!_standalone)일 때만 루프를 해제하고 시퀀스를 강제 종료합니다.
            // 스탠드얼론 모드라면 외부에서 인터럽트가 걸려도 아무 작업도 하지 않고 StandaloneUpdate가 계속 돌도록 내버려 둡니다.
            if (!_standalone)
            {
                _standaloneUpdateHandle?.Dispose();
                _standaloneUpdateHandle = null;

                _sequence.Stop();

                // 일반 모드에서는 인터럽트 시 미발사 투사체를 파괴하는 로직을 이곳에 추가하는 것이 좋습니다.
                for (int i = 0; i < _spikes.Length; i++)
                {
                    if (_spikes[i] != null)
                    {
                        UnityEngine.Object.Destroy(_spikes[i].gameObject);
                    }
                }
            }
        }
    }
}
