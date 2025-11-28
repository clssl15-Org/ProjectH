using System;
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
        private bool _haltOnActionEnd;
        private Action _opened;

        // Content
        public Idle(MonsterActionType actionType = MonsterActionType.Idle, bool haltOnActionEnd = true, Action opened = null)
            : this(actionType.ToString(), haltOnActionEnd, opened) { }
        public Idle(string monsterAction, bool haltOnActionEnd = true, Action opened = null)
        {
            AbortPolicies = AbortPolicies.Self;
            _monsterAction = monsterAction;
            _haltOnActionEnd = haltOnActionEnd;
            _opened = opened;
        }

        public override bool CheckCondition() =>
            !(bool)Blackboard.Properties[ITwinBoss.IsAwake];

        protected override void OnOpen(object[] _)
        {
            if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result =>
                {
                    if(_haltOnActionEnd)
                        Complete();
                },
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

            _opened?.Invoke();
        }
    }
}
