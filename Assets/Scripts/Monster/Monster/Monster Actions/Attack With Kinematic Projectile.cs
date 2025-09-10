using System;
using UnityEngine;

namespace MonsterActions
{
    internal class AttackWithKinematicProjectile : MonsteActionState
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
        private Func<Vector2> getDirection;

        private LaunchInfo currentLaunchInfo;
        private float currentPlaytime;
        private bool launched;


        // Content
        public AttackWithKinematicProjectile(
            KinematicProjectileLauncher launcher,
            Func<LaunchInfo> getLaunchInfo,
            Func<Vector2> getDirection,
            MonsterAction baseAction = MonsterAction.Attack)
            : this(launcher, getLaunchInfo, getDirection, baseAction.ToString()) { }

        public AttackWithKinematicProjectile(
            KinematicProjectileLauncher launcher,
            Func<LaunchInfo> getLaunchInfo,
            Func<Vector2> getDirection,
            string baseAction)
            : base(baseAction)
        {
            this.launcher = launcher;
            this.getLaunchInfo = getLaunchInfo;
            this.getDirection = getDirection;
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
                launcher.LaunchWithRotation(currentLaunchInfo.speed, getDirection());
            }
        }
    }
}
