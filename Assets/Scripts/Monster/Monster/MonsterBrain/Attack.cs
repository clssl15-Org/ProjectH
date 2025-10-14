using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal class Attack : BTNode<IMonster, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;


        // Content
        public Attack(MonsterActionType monsterAction = MonsterActionType.Attack) : this(monsterAction.ToString()) { }
        public Attack(string monsterAction) => _monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result => Complete(result)),
                out var reason,
                allowRestart: true))
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {nameof(Attack)} 상태로 진입할 수 없습니다.\n{reason}"));

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
