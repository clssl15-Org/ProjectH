using Actors.Monsters.Actions;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class Patrol : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Front
        public float MinPatrolTime { get; set; } = 0.5f;
        public float MaxPatrolTime { get; set; } = 2f;

        // Internal
        private readonly string _monsterAction;
        private float _remainingTime;


        // Content
        public Patrol(MonsterActionType monsterAction = MonsterActionType.Walk) : this(monsterAction.ToString()) { }
        public Patrol(string monsterAction) => _monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            _remainingTime = Random.Range(MinPatrolTime, MaxPatrolTime);

            if (Owner.Direction != Direction.Left && Owner.Direction != Direction.Right)
                Owner.Direction = Random.Range(0, 2) == 0 ? Direction.Left : Direction.Right;

            if (!Owner.TryDoAction(new(_monsterAction), out var reason)
                 && reason.ResultType != ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 Patrol 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0)
                Complete();

            if (!Owner.TryMove())
                Owner.Direction = Owner.Direction.Flip();
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.StopMoving();
        }
    }
}
