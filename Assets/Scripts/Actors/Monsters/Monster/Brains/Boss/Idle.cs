using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    internal class Idle : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;
        private readonly string _isAwakeKey;
        private bool _haltOnActionEnd;
        private Action _opened;

        // Content
        public Idle(string isAwakeKey, MonsterActionType actionType = MonsterActionType.Idle, bool haltOnActionEnd = true, Action opened = null)
            : this(actionType.ToString(), isAwakeKey, haltOnActionEnd, opened) { }
        public Idle(string monsterAction, string isAwakeKey, bool haltOnActionEnd = true, Action opened = null)
        {
            AbortPolicies = AbortPolicies.Self;

            _monsterAction = monsterAction;
            _isAwakeKey = isAwakeKey;
            _haltOnActionEnd = haltOnActionEnd;
            _opened = opened;
        }

        public override bool CheckCondition() =>
            !(bool)Blackboard.Properties[_isAwakeKey];

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
                return;
            }

            Owner.IgnorePlayerInteraction = true;
            _opened?.Invoke();
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.IgnorePlayerInteraction = false;
        }
    }
}
