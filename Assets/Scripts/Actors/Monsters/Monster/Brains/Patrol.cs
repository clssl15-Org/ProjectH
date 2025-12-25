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
        private int _turnCount;


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
                return;
            }

            _turnCount = 0;
        }

        protected override void OnTick()
        {
            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0)
            {
                Complete();
                return;
            }

            if (!Owner.TryMove())
            {
                _turnCount++;
                if (_turnCount >= 2)
                {
                    // 이동 공간 없음
                    Complete(false);
                    return;
                }

                Owner.Direction = Owner.Direction.Flip();
            }
            else
                _turnCount = 0;
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.StopMoving();
        }
    }
}
