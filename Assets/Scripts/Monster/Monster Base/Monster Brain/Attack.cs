using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Attack : BTNode<Monster, MonsterBlackboard>
    {
        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(MonsterAction.Attack, out var reason, result => Complete(result)))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"Attack 행동에 실패하였기 때문에 Attack 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }

            Blackboard.Committing = true;
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Blackboard.Committing = false;
        }
    }
}
