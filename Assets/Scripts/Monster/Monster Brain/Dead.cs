using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class Dead : BTNode<Monster, MonsterBlackboard>
    {
        // Internal
        private readonly string monsterAction;


        // Content
        public Dead(MonsterAction monsterAction = MonsterAction.Dead) : this(monsterAction.ToString()) { }
        public Dead(string monsterAction) => this.monsterAction = monsterAction;

        public override bool CheckCondition() => Owner.IsAlive;

        protected override void OnOpen(object[] _)
        {
            Owner.IsAlive = false;

            if (!Owner.TryDoAction(monsterAction, out var reason,
                result => Complete(),
                allowRestart: true,
                stayTimeAfterFinised: 1f))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 Dead 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.Die();
        }
    }
}
