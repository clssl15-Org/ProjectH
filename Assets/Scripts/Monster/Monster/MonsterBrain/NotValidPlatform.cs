using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class NotValidPlatform : BTNode<Monster, MonsterBlackboard>
    {
        private readonly string monsterAction;


        public NotValidPlatform(MonsterActionType monsterAction = MonsterActionType.Idle) : this(monsterAction.ToString()) { }
        public NotValidPlatform(string monsterAction) => this.monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(new(monsterAction), out var reason)
                 && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 NotValidPlatform 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            if (Owner.PlatformDetector.TryGetCurrentPlatformId(out var platformId))
            {
                Owner.BelongingPlatform = platformId;
                RetickNow = true;

                Complete();
            }
        }
    }
}
