using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Attack : BTNode<Monster, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;


        // Content
        public Attack(MonsterAction monsterAction = MonsterAction.Attack) : this(monsterAction.ToString()) { }
        public Attack(string monsterAction) => _monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(_monsterAction, out var reason, result => Complete(result), allowRestart: true))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

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
