using System;

namespace MonsterActions
{
    internal class ManualHit : MonsteActionState
    {
        // Internal
        private DamageHandler damageHandler;


        // Content
        public ManualHit(DamageHandler damageHandler) : base(MonsterAction.Hit.ToString())
        {
            this.damageHandler = damageHandler;
            throw new NotImplementedException();
        }

        protected override void OnEnter(params object[] inputs)
        {
            SetInputs(inputs);

            stayTimeAfterFinished = 0f;
            remainingTime = Owner.InvincibleTime;

            isCompleted = false;
        }
    }
}
