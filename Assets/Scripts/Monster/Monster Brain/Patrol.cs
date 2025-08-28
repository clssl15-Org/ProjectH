using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Patrol : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float MinPatrolTime { get; set; } = 0.5f;
        public float MaxPatrolTime { get; set; } = 2f;

        // Internal
        private readonly string monsterAction;
        private float remainingTime;


        // Content
        public Patrol(MonsterAction monsterAction = MonsterAction.Walk) : this(monsterAction.ToString()) { }
        public Patrol(string monsterAction) => this.monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            remainingTime = Random.Range(MinPatrolTime, MaxPatrolTime);

            if (Owner.Direction != Direction.Left && Owner.Direction != Direction.Right)
                Owner.Direction = Random.Range(0, 2) == 0 ? Direction.Left : Direction.Right;

            if (!Owner.TryDoAction(monsterAction, out var reason)
                 && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 Patrol 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
                Complete();

            if (!Owner.TryMove())
            {
                Owner.Direction = (Owner.Direction == Direction.Left)
                    ? Direction.Right
                    : Direction.Left;
            }
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.StopMoving();
        }
    }
}
