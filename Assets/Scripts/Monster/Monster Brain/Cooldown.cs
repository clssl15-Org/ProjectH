using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class Cooldown : BTNode<Monster, MonsterBlackboard>
    {
        // Internal
        private readonly string monsterAction;
        private float? remainingCooldownTime;


        // Content
        public Cooldown(MonsterAction monsterAction = MonsterAction.Idle) : this(monsterAction.ToString()) { }
        public Cooldown(string monsterAction) => this.monsterAction = monsterAction;


        protected override void OnOpen(object[] _)
        {
            remainingCooldownTime = Owner.AttackCooltime;

            if (!Owner.TryDoAction(monsterAction, out var reason)
                && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 Cooldown 상태로 진입할 수 없습니다.\n{reason}"));

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
