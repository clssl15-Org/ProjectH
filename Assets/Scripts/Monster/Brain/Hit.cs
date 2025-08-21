using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class Hit : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float InvincibleTime { get; set; }

        // Internal
        private float remainingTime;


        // Content
        public Hit(float invincibleTime = 1)
        {
            AbortPolicy = AbortPolicies.LowerPriority;
            InvincibleTime = invincibleTime;
        }

        protected override bool CheckCondition()
        {
            if (Blackboard.Committing)
                return false;

            if (!Blackboard.IsDamaged)
                return false;

            return true;
        }

        protected override void OnOpen()
        {
            Owner.HP -= Blackboard.TakenDamage;
            Blackboard.ClearDamage();

            remainingTime = InvincibleTime;

            if (!Owner.TryDoAction(MonsterAction.Hit))
                Complete();
        }

        protected override void OnTick()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
                Complete();
        }
    }
}
