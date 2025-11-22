using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class Await : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        public float Duration { get; set; }
        private float _remaining;
        private string _monsterAction = string.Empty;

        public Await(float duration) => Duration = duration;

        public Await(MonsterActionType actionType, float duration) : this(actionType.ToString(), duration) { }
        public Await(string actionName, float duration) : this(duration) => _monsterAction = actionName;

        protected override void OnOpen(params object[] _)
        {
            _remaining = Duration;

            if (!string.IsNullOrEmpty(_monsterAction))
            {
                if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result => Complete(),
                Inputs: new[]
                {
                    new PlayAnimation.AnimationPlayInfo()
                }),
                out var reason,
                allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{_monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }
            }
        }

        protected override void OnTick()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) Complete();
        }
    }
}
