using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal class Rest : BTNode<IMonster, MonsterBlackboard>
    {
        // Front
        public bool HasExitTime { get; set; } = true;
        public float MinRestTime { get; set; } = 0.5f;
        public float MaxRestTime { get; set; } = 2f;

        // Internal
        private readonly string _monsterAction;
        private float _remainingTime;


        // Content
        public Rest(MonsterActionType monsterAction = MonsterActionType.Idle) : this(monsterAction.ToString()) { }
        public Rest(string monsterAction) => _monsterAction = monsterAction;

        protected override void OnOpen(object[] _)
        {
            if (HasExitTime)
                _remainingTime = Random.Range(MinRestTime, MaxRestTime);

            if (Owner.Rigidbody.bodyType == RigidbodyType2D.Dynamic)
                Owner.Rigidbody.velocity = new Vector2
                {
                    x = 0,
                    y = Owner.Rigidbody.velocity.y,
                };

            if (!Owner.TryDoAction(new(_monsterAction), out var reason)
                && reason.Result != ActionResult.ResultType.AlreadyDoing)
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 Rest 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnTick()
        {
            if (!HasExitTime)
                return;

            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0)
                Complete();
        }
    }
}
