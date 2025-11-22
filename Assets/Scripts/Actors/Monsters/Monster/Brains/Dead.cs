using System;
using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;
    
namespace Actors.Monsters.Brains
{
    internal class Dead : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Front
        public float StayTimeAfterFinised { get; set; } = 0f;
        public bool DestroyOwnerOnCompleted { get; set; } = true;

        // Internal
        private readonly string _monsterAction;
        private MonsterConditionData _notification;
        private Action _opening;


        // Content
        public Dead(MonsterActionType monsterAction = MonsterActionType.Dead, Action opening = null)
            : this(monsterAction.ToString(), opening) { }
        public Dead(string monsterAction, Action opening = null)
        {
            _monsterAction = monsterAction;
            _opening = opening;
        }

        public override bool CheckCondition() => Owner.IsAlive;

        protected override void OnOpen(object[] _)
        {
            Owner.IsAlive = false;
            Owner.IgnorePlayerInteraction = true;

            _opening?.Invoke();

            _notification = new MonsterConditionData(MonsterCondition.Die);
            Owner.NotifyCondition(_notification);

            if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result => Complete(),
                Inputs: new[]
                {
                    new PlayAnimation.AnimationPlayInfo(DelayAfterPlay: StayTimeAfterFinised)
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

            if (DestroyOwnerOnCompleted)
                Owner.Destroy();
        }
    }
}
