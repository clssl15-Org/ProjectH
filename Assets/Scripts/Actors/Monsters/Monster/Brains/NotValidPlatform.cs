using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class NotValidPlatform : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        private readonly string _monsterAction;


        public NotValidPlatform(MonsterActionType monsterAction = MonsterActionType.Idle) : this(monsterAction.ToString()) { }
        public NotValidPlatform(string monsterAction) => this._monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(new(_monsterAction), out var reason)
                 && reason.ResultType != ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 NotValidPlatform 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            if (Owner.PlatformDetector.TryGetCurrentPlatformId(out var platformId))
            {
                Owner.CurrentPlatform = platformId;
                RetickNow = true;

                Complete();
            }
        }
    }
}
