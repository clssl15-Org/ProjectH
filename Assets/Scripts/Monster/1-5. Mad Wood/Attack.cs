using UniEngine.StateMachines.BT;
using UnityEngine;

public partial class MadWood : Monster
{
    private class MadWoodAttack : BTNode<Monster, MonsterBlackboard>
    {
        private const string PreviousAttackMode = "PreviousAttackMode";

        public MadWoodAttack() : base(name: MonsterAction.Attack.ToString()) { }

        protected override void OnOpen(object[] _)
        {
            if (!Blackboard.Properties.TryGetValue(PreviousAttackMode, out var _currentAttackMode))
                Blackboard.Properties[PreviousAttackMode] = AttackMode.LandAttack;

            var currentAttackMode = Blackboard.Properties[PreviousAttackMode] is AttackMode.DefaultAttack
                ? AttackMode.LandAttack
                : AttackMode.DefaultAttack;

            if (!Owner.TryDoAction(currentAttackMode.ToString(), out var reason, result => Complete(result)))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{currentAttackMode.ToString()} 행동에 실패하였기 때문에 Attack 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }

            Blackboard.Properties[PreviousAttackMode] = currentAttackMode;
            Blackboard.Committing = true;
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Blackboard.Committing = false;
        }
    }
}
