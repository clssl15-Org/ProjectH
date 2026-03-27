using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    internal class TwinBossExhaustedBrain : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Front
        public event Action Opened;

        // Internal
        private readonly string _monsterAction;
        private MonsterConditionData _notification;
        private float _originalGravityScale;    

        // Content
        public TwinBossExhaustedBrain() : this(nameof(TwinBossExhaustedBrain)) { }
        public TwinBossExhaustedBrain(string monsterAction) => _monsterAction = monsterAction;

        public override bool CheckCondition() => Owner.IsAlive;

        protected override void OnOpen(object[] _)
        {
            Owner.IgnorePlayerInteraction = true;
            ((ITwinBoss)Owner).IsExhausted = true;

            _originalGravityScale = Owner.Rigidbody.gravityScale;
            Owner.Rigidbody.gravityScale = 1f;

            _notification = new MonsterConditionData(MonsterCondition.General, nameof(TwinBossExhaustedBrain));
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

            Opened?.Invoke();
        }

        public TwinBossExhaustedBrain OnOpened(Action opened)
        {
            Opened += opened;
            return this;
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Owner.IgnorePlayerInteraction = false;
            ((ITwinBoss)Owner).IsExhausted = false;

            Owner.Rigidbody.gravityScale = _originalGravityScale;

            _notification?.Complete();
            _notification = null;
        }
    }
}
