using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal class Adjusting : BTNode<IMonster, MonsterBlackboard>
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
                 && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {nameof(Adjusting)} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            if (!Blackboard.Moved)
                Complete(false);
        }
    }
}
