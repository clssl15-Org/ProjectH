using System;
using UnityEngine;
using static KinematicProjectileLauncher;

namespace MonsterActions
{
    internal class AttackWithKinematicProjectile : MonsterActionComponent
    {
        // Front
        public record LaunchInfo(float LaunchTime, float Speed);

        // Internal
        private KinematicProjectileLauncher _launcher;
        private Func<LaunchInfo> _getLaunchInfo;
        private LaunchType _launchType;
        private Func<Vector2[]> _getDirections;

        private LaunchInfo _currentLaunchInfo;
        private bool _launched;


        // Content
        public AttackWithKinematicProjectile(
            KinematicProjectileLauncher launcher,
            Func<LaunchInfo> getLaunchInfo,
            LaunchType launchType,
            Func<Vector2[]> getDirections)
        {
            _launcher = launcher;
            _getLaunchInfo = getLaunchInfo;
            _launchType = launchType;
            _getDirections = getDirections;
        }

        protected override void OnEnter(object input)
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
                    case LaunchType.Rotation:
                        _launcher.LaunchWithRotation(_currentLaunchInfo.Speed, _getDirections()[0]);
                        break;

                    case LaunchType.LocalRotation:
                        _launcher.LaunchWithLocalRotation(_currentLaunchInfo.Speed, _getDirections()[0]);
                        break;

                    case LaunchType.Directions:
                        _launcher.LaunchWithDirections(_currentLaunchInfo.Speed, _getDirections());
                        break;

                    default:
                        throw new InvalidOperationException(Owner.Ctx(
                            $"({GetType().Name}) 알 수 없는 {nameof(_launchType)} '{_launchType}'에 대한 발사를 수행할 수 없습니다."));
                }

                Interrupt(InterruptType.Completed);
            }
        }
    }
}
