using System;
using UnityEngine;
using static KinematicProjectileLauncher;

namespace MonsterActions
{
    internal class AttackWithKinematicProjectile : MonsterActionState
    {
        // Front
        public struct LaunchInfo
        {
            public float launchTime;
            public float speed;

            public LaunchInfo(float launchTime, float speed)
            {
                this.launchTime = launchTime;
                this.speed = speed;
            }
        }

        // Internal
        private KinematicProjectileLauncher launcher;
        private Func<LaunchInfo> getLaunchInfo;
        private LaunchType launchType;
        private Func<Vector2[]> getDirections;

        private LaunchInfo currentLaunchInfo;
        private float currentPlaytime;
        private bool launched;


        // Content
        public AttackWithKinematicProjectile(
            KinematicProjectileLauncher launcher,
            Func<LaunchInfo> getLaunchInfo,
            LaunchType launchType,
            Func<Vector2[]> getDirections,
            MonsterAction baseAction = MonsterAction.Attack)
            : this(launcher, getLaunchInfo, launchType, getDirections, baseAction.ToString()) { }

        public AttackWithKinematicProjectile(
            KinematicProjectileLauncher launcher,
            Func<LaunchInfo> getLaunchInfo,
            LaunchType launchType,
            Func<Vector2[]> getDirections,
            string baseAction)
            : base(baseAction)
        {
            this.launcher = launcher;
            this.getLaunchInfo = getLaunchInfo;
            this.launchType = launchType;
            this.getDirections = getDirections;
        }

        protected override void OnEnter(params object[] inputs)
        {
            currentPlaytime = 0f;
            launched = false;

            currentLaunchInfo = getLaunchInfo();
            base.OnEnter(inputs);
        }

        protected override void OnUpdate()
        {
            currentPlaytime += Time.deltaTime;

            if (!launched && currentPlaytime >= currentLaunchInfo.launchTime)
            {
                launched = true;

                switch (launchType)
                {
                    case LaunchType.Rotation:
                        launcher.LaunchWithRotation(currentLaunchInfo.speed, getDirections()[0]);
                        break;

                    case LaunchType.LocalRotation:
                        launcher.LaunchWithLocalRotation(currentLaunchInfo.speed, getDirections()[0]);
                        break;

                    case LaunchType.Directions:
                        launcher.LaunchWithDirections(currentLaunchInfo.speed, getDirections());
                        break;

                    default:
                        throw new InvalidOperationException(Owner.Ctx(
                            $"({GetType().Name}) 알 수 없는 {nameof(launchType)} '{launchType}'에 대한 발사를 수행할 수 없습니다."));
                }
            }
        }
    }
}
