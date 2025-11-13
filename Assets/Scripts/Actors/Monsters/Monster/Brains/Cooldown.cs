using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class Cooldown : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;
        private float? _remainingCooldownTime;


        // Content
        public Cooldown(MonsterActionType monsterAction = MonsterActionType.Idle) : this(monsterAction.ToString()) { }
        public Cooldown(string monsterAction) => _monsterAction = monsterAction;


        protected override void OnOpen(object[] _)
        {
            _remainingCooldownTime = Owner.StatsInfo.AttackCooltime;

            if (!Owner.TryDoAction(new(_monsterAction), out var reason)
                && reason.ResultType != ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {nameof(Cooldown)} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            if (!_remainingCooldownTime.HasValue)
                return;

            _remainingCooldownTime -= Time.deltaTime;

            if (_remainingCooldownTime <= 0)
                Complete();
        }
    }
}
