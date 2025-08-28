using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Rest : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float MinRestTime { get; set; } = 0.5f;
        public float MaxRestTime { get; set; } = 2f;

        // Internal
        private readonly string monsterAction;
        private float remainingTime;


        // Content
        public Rest(MonsterAction monsterAction = MonsterAction.Idle) : this(monsterAction.ToString()) { }
        public Rest(string monsterAction) => this.monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            remainingTime = Random.Range(MinRestTime, MaxRestTime);

            Owner.Rigidbody.velocity = new Vector2
            {
                x = 0,
                y = Owner.Rigidbody.velocity.y,
            };

            if (!Owner.TryDoAction(monsterAction, out var reason)
                && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 Rest 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
                Complete();
        }
    }
}
