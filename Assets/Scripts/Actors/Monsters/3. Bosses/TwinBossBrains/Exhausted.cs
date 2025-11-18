using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    internal class Exhausted : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;
        private MonsterConditionData _notification;


        // Content
        public Exhausted() : this(nameof(Exhausted)) { }
        public Exhausted(string monsterAction) => _monsterAction = monsterAction;

        public override bool CheckCondition() => Owner.IsAlive;

        protected override void OnOpen(object[] _)
        {
            _notification = new MonsterConditionData(MonsterCondition.General, nameof(Exhausted));
            Owner.NotifyCondition(_notification);

            if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result => Complete(),
                Inputs: new[]
                {
                    new PlayAnimation.AnimationPlayInfo(DelayAfterPlay: -1f)
                }),
                out var reason,
                allowRestart: true))
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            _notification?.Complete();
            _notification = null;
        }
    }
}
