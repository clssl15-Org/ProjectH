using System;
using UnityEngine;

namespace Actors.Monsters.Actions
{
    internal class AttackWithKinematicProjectile : MonsterActionComponent
    {
        // Front
        public record LaunchInfo(float LaunchTime, float Speed);

        // Internal
        private KinematicProjectileLauncher _launcher;
        private Func<LaunchInfo> _getLaunchInfo;
        private KinematicProjectileLaunchType _launchType;
        private Func<Vector2[]> _getDirections;

        private LaunchInfo _currentLaunchInfo;
        private bool _launched;


        // Content
        public AttackWithKinematicProjectile(
            KinematicProjectileLauncher launcher,
            Func<LaunchInfo> getLaunchInfo,
            KinematicProjectileLaunchType launchType,
            Func<Vector2[]> getDirections)
        {
            _launcher = launcher;
            _getLaunchInfo = getLaunchInfo;
            _launchType = launchType;
            _getDirections = getDirections;
        }

        protected override void OnEnter(float _, object __)
        {
            _currentLaunchInfo = _getLaunchInfo();
            _launched = false;
        }

        protected override void OnUpdate(float elapsedTime)
        {
            if (!_launched && elapsedTime >= _currentLaunchInfo.LaunchTime)
            {
                _launched = true;

                switch (_launchType)
                {
                    case KinematicProjectileLaunchType.Rotation:
                        _launcher.LaunchWithRotation(_currentLaunchInfo.Speed, _getDirections()[0]);
                        break;

                    case KinematicProjectileLaunchType.LocalRotation:
                        _launcher.LaunchWithLocalRotation(_currentLaunchInfo.Speed, _getDirections()[0]);
                        break;

                    case KinematicProjectileLaunchType.Directions:
                        _launcher.LaunchWithDirections(_currentLaunchInfo.Speed, _getDirections());
                        break;

                    default:
                        throw new InvalidOperationException(Owner.FormatLogMessage(
                            $"({GetType().Name}) 알 수 없는 {nameof(_launchType)} '{_launchType}'에 대한 발사를 수행할 수 없습니다."));
                }

                Interrupt(InterruptType.Completed);
            }
        }
    }
}
