using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Adjusting : BTNode<Monster, MonsterBlackboard>
    {
        // Internal
        private readonly string monsterAction;


        // Content
        public Adjusting(MonsterAction monsterAction = MonsterAction.Run) : this(monsterAction.ToString()) { }
        public Adjusting(string monsterAction)
        {
            AbortPolicy = AbortPolicies.StopOnFailure;
            this.monsterAction = monsterAction;
        }

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(monsterAction, out var reason)
                 && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

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
