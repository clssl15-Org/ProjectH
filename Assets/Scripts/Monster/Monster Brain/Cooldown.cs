using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class Cooldown : BTNode<Monster, MonsterBlackboard>
    {
        public float? CooldownTime { get; set; }
        private float? remainingCooldownTime;


        // Content
        public Cooldown(float? cooldownTime = 0.5f)
        {
            CooldownTime = cooldownTime;
        }

        protected override void OnOpen(object[] _)
        {
            remainingCooldownTime = CooldownTime;

            if (!Owner.TryDoAction(MonsterAction.Idle, out var reason)
                && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.Ctx(
                    $"Idle 행동에 실패하였기 때문에 Cooldown 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            if (!remainingCooldownTime.HasValue)
                return;

            remainingCooldownTime -= Time.deltaTime;

            if (remainingCooldownTime <= 0)
                Complete();
        }
    }
}
