using MonsterActions;
using UnityEngine;

public partial class Crow
{
    private class CrowAttackAction : MonsteActionState
    {
        // Internal
        private float currentPlaytime;
        private bool launched;


        // Content
        public CrowAttackAction() : base(MonsterAction.Attack) { }

        protected override void OnEnter(params object[] inputs)
        {
            currentPlaytime = 0f;
            launched = false;

            base.OnEnter(inputs);
        }

        protected override void OnUpdate()
        {
            var owner = (Crow)Owner;

            currentPlaytime += Time.deltaTime;
            if (!launched && currentPlaytime >= owner.launchTime)
            {
                launched = true;

                owner.projectileLauncher.LaunchWithRotation(
                    owner.projectileSpeed,
                    owner.DetectedPlayer.transform.position - owner.transform.position);
            }
        }
    }
}
