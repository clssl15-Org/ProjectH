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
        protected override bool CheckCondition() => Owner.IsAttacking;

        protected override void OnOpen(object[] _)
        {
            Owner.IsAttacking = true;
            Owner.DoAction(MonsterAction.Attack, Complete);
            Blackboard.Committing = true;
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.DoAction(MonsterAction.Idle);
            Blackboard.Committing = false;
        }
    }
}
