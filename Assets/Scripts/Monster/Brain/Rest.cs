using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Rest : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float MinRestTime { get; set; } = 0.5f;
        public float MaxRestTime { get; set; } = 2f;

        // Internal
        private float remainingTime;


        // Content
        protected override void OnOpen(object[] _)
        {
            remainingTime = Random.Range(MinRestTime, MaxRestTime);

            Owner.Rigidbody.velocity = new Vector2
            {
                x = 0,
                y = Owner.Rigidbody.velocity.y,
            };

            Owner.DoAction(MonsterAction.Idle);
        }

        protected override void OnTick()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
                Complete();
        }
    }
}
