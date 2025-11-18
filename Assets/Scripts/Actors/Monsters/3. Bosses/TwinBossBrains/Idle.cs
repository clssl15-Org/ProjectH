using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    internal class Idle : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;

        // Content
        public Idle() : this(nameof(Idle)) { }
        public Idle(string monsterAction)
        {
            AbortPolicies = AbortPolicies.Self;
            _monsterAction = monsterAction;
        }

        public override bool CheckCondition() =>
            !(bool)Blackboard.Properties[ITwinBoss.IsAwaken];

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result => Complete(),
                Inputs: new[]
                {
                    new PlayAnimation.AnimationPlayInfo()
                }),
                out var reason,
                allowRestart: true))
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }
        }
    }
}
