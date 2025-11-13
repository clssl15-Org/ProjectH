using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class DeadEnd : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Front
        public float WaitingTime { get; set; } = 1f;

        // Internal
        private readonly string _monsterAction;
        private float _remainingTime;


        // Content
        public DeadEnd(MonsterActionType monsterAction = MonsterActionType.Idle) : this(monsterAction.ToString()) { }
        public DeadEnd(string monsterAction)
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
                    $"{_monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }

            _remainingTime = WaitingTime;
        }

        protected override void OnTick()
        {
            if (Blackboard.Moved)
            {
                Complete(false);
                return;
            }

            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0f)
                Complete();
        }
    }
}
