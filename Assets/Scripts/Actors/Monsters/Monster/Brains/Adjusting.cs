using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class Adjusting : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;


        // Content
        public Adjusting(MonsterActionType monsterAction = MonsterActionType.Run) : this(monsterAction.ToString()) { }
        public Adjusting(string monsterAction)
        {
            AbortPolicies = AbortPolicies.StopOnFailure;
            _monsterAction = monsterAction;
        }

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(new(_monsterAction), out var reason)
                 && reason.ResultType != ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 '{nameof(Adjusting)}' 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            if (!Blackboard.IsMoved)
                Complete(false);
        }
    }
}
