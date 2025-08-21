using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Engaged : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float TargetAttackRange { get; set; } = 3f;
        public float UpperRangeTolerance { get; set; } = 0.1f;
        public float LowerRangeTolerance { get; set; } = 0.3f;


        // Content
        public Engaged(bool contact)
        {
            AbortPolicy = AbortPolicies.Self;

            if (contact)
            {
                TargetAttackRange = 0f;
                UpperRangeTolerance = 1f;
            }
        }

        protected override void OnOpen()
        {
            Owner.TryDoAction(MonsterAction.Run);
        }

        protected override void OnTick()
        {
            var posDelta = Owner.transform.position.x - Owner.DetectedPlayer.transform.position.x;
            Owner.Direction = posDelta > 0 ? Direction.Left : Direction.Right;

            var rangeDelta = Mathf.Abs(posDelta) - TargetAttackRange;

            if (rangeDelta < -LowerRangeTolerance)
            {
                Owner.TryMove(posDelta > 0 ? Direction.Right : Direction.Left);
                return;
            }
            if (rangeDelta > UpperRangeTolerance)
            {
                Owner.TryMove();
                return;
            }

            Complete();
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Blackboard.WasEngaged = true;
        }
    }
}
