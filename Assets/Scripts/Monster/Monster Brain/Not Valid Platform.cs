using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class NotValidPlatform : BTNode<Monster, MonsterBlackboard>
    {
        public NotValidPlatform()
        {
            AbortPolicy = AbortPolicies.LowerPriority;
        }

        public override bool CheckCondition()
        {
            if (Blackboard.Committing)
                return false;

            if (Owner.PlatformDetector.CheckPlatform(
                Direction.Center, Owner.BelongingPlatform, out _))
            {
                return false;
            }

            return true;
        }

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(MonsterAction.Idle, out var reason)
                 && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.Ctx(
                    $"Idle 행동에 실패하였기 때문에 NotValidPlatform 상태로 진입할 수 없습니다.\n{reason}"));

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
