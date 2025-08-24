using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class Cooldown : BTNode<Monster, MonsterBlackboard>
    {
        public float CooldownTime { get; set; }
        private float remainingCooldownTime;


        // Content
        public Cooldown(float cooldownTime = 1f)
        {
            CooldownTime = cooldownTime;
        }

        protected override void OnOpen(object[] _)
        {
            remainingCooldownTime = CooldownTime;
            Owner.DoAction(MonsterAction.Idle);
        }

        protected override void OnTick()
        {
            remainingCooldownTime -= Time.deltaTime;

            if (remainingCooldownTime <= 0)
                Complete();
        }
    }
}
