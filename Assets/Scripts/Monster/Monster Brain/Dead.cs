using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Dead : BTNode<Monster, MonsterBlackboard>
    {
        public override bool CheckCondition() => Owner.IsAlive;

        protected override void OnOpen(object[] _)
        {
            Owner.IsAlive = false;

            if (!Owner.TryDoAction(MonsterAction.Dead, out var reason, result => Complete(),
                allowRestart: true, playTime: 1))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"Dead 행동에 실패하였기 때문에 Dead 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.Die();
        }
    }
}
