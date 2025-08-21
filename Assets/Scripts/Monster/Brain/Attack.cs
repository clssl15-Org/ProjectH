using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Attack : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float TargetAttackRange { get; set; } = 3f;
        public float UpperRangeTolerance { get; set; } = 0.1f;
        public float LowerRangeTolerance { get; set; } = 0.3f;


        // Content
        protected override bool CheckCondition()
        {
            return Owner.TryDoAction(MonsterAction.Attack, Complete);
        }

        protected override void OnOpen()
        {
            Blackboard.Committing = true;
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.TryDoAction(MonsterAction.None);
            Blackboard.Committing = false;
        }
    }
}
