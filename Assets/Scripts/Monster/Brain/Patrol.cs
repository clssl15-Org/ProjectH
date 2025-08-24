using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Patrol : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float MinPatrolTime { get; set; } = 0.5f;
        public float MaxPatrolTime { get; set; } = 2f;

        // Internal
        private float remainingTime;


        // Content
        protected override void OnOpen(object[] _)
        {
            remainingTime = Random.Range(MinPatrolTime, MaxPatrolTime);

            if (Owner.Direction != Direction.Left && Owner.Direction != Direction.Right)
                Owner.Direction = Random.Range(0, 2) == 0 ? Direction.Left : Direction.Right;

            Owner.DoAction(MonsterAction.Walk);
        }

        protected override void OnTick()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
                Complete();


            if (!Owner.TryMove())
            {
                Owner.Direction = (Owner.Direction == Direction.Left)
                    ? Direction.Right
                    : Direction.Left;
            }
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.Rigidbody.velocity = new Vector2
            {
                x = 0,
                y = Owner.Rigidbody.velocity.y,
            };
        }
    }
}
