using System;
using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public class SpikeLauncher
    {
        public float AimingTime { get; set; } = 0.5f;
        public float AimingSpeed { get; set; } = 2f;

        private KinematicProjectile _spike;
        private float _startAngle;
        private Func<Vector2> _getTargetPosition;
        private float _spikeSpeed;

        private IDisposable _updateHandle;
        private float _currentTime;


        public SpikeLauncher(
            KinematicProjectile spike,
            Func<Vector2> getTargetPosition,
            float spikeSpeed)
        {
            _spike = spike;
            _startAngle = _spike.Rigidbody.rotation;
            _getTargetPosition = getTargetPosition;
            _spikeSpeed = spikeSpeed;

            _currentTime = 0;
        }

        public void Fire()
        {
            _spike.Rigidbody.velocity = Vector2.zero;
            _spike.Rigidbody.angularVelocity = 0f;

            _updateHandle = Loco.Subscribe(Update);
        }

        private void Update()
        {
            var t = _currentTime / AimingTime;
            if (t >= 1)
            {
                _spike.Launch(_getTargetPosition() - (Vector2)_spike.transform.position, _spikeSpeed);
                _updateHandle.Dispose();
                return;
            }

            var dir = _getTargetPosition() - (Vector2)_spike.transform.position;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.down;
            var targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

            var angle = Mathf.LerpAngle(_startAngle, targetAngle, Mathf.Clamp01(t * AimingSpeed));
            _spike.Rigidbody.MoveRotation(angle);

            _currentTime += Time.deltaTime;
        }
    }
}
