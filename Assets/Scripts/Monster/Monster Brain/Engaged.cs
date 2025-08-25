using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Engaged : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float TargetAttackRange { get; set; } = 3f;
        public float UpperRangeTolerance { get; set; } = 0.1f;
        public float LowerRangeTolerance { get; set; } = 0.3f;

        // Private
        private MonsterAction monsterAction;


        // Content
        public Engaged(bool contact = false, MonsterAction monsterAction = MonsterAction.Run)
        {
            AbortPolicy = AbortPolicies.Self;

            if (contact)
            {
                TargetAttackRange = 0f;
                UpperRangeTolerance = 1f;
            }

            this.monsterAction = monsterAction;
        }

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(monsterAction, out var reason)
                 && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 Engaged 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            var posDelta = Owner.transform.position.x - Owner.DetectedPlayer.transform.position.x;
            Owner.Direction = posDelta > 0 ? Direction.Left : Direction.Right;

            var rangeDelta = Mathf.Abs(posDelta) - TargetAttackRange;

            if (rangeDelta < -LowerRangeTolerance)
            {
                Owner.TryMove(posDelta > 0 ? Direction.Right : Direction.Left);
                return;
            }
            if (rangeDelta > UpperRangeTolerance)
            {
                Owner.TryMove();
                return;
            }

            Complete();
        }
    }
}
