using System.Collections;
using System.Collections.Generic;
using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class DeadEnd : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float WaitingTime { get; set; } = 1f;

        // Internal
        private readonly string monsterAction;
        private float remainingTime;


        // Content
        public DeadEnd(MonsterAction monsterAction = MonsterAction.Idle) : this(monsterAction.ToString()) { }
        public DeadEnd(string monsterAction)
        {
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

            remainingTime = WaitingTime;
        }

        protected override void OnTick()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
                Complete();
        }
    }
}
