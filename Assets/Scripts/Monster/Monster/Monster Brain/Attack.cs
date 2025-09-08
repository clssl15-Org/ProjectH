using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Attack : BTNode<Monster, MonsterBlackboard>
    {
        // Internal
        private readonly string monsterAction;


        // Content
        public Attack(MonsterAction monsterAction = MonsterAction.Attack) : this(monsterAction.ToString()) { }
        public Attack(string monsterAction) => this.monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(monsterAction, out var reason, result => Complete(result)))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 Attack 상태로 진입할 수 없습니다.\n{reason}"));

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
