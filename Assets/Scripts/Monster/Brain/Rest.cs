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
        protected override bool CheckCondition()
        {
            // 이전이 Engaged 상태였으면 즉시 휴식
            if (Blackboard.WasEngaged)
            {
                Blackboard.WasEngaged = false;
                return true;
            }

            // 33%의 확률로 Rest
            return Random.Range(0, 3) == 0;
        }


        protected override void OnOpen()
        {
            remainingTime = Random.Range(MinRestTime, MaxRestTime);

            Owner.Rigidbody.velocity = new Vector2
            {
                x = 0,
                y = Owner.Rigidbody.velocity.y,
            };

            Owner.TryDoAction(MonsterAction.Idle);
        }

        protected override void OnTick()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
                Complete();
        }
    }
}
