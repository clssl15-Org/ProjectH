using System;
using Actors.Monsters.Actions;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Belia
    {
        private class BeliaAttackBrain : BTNode<IMonsterInternal, Brains.MonsterBlackboard>
        {
            // Internal
            private MonsterConditionData _notification;


            // Content
            public BeliaAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(params object[] _)
            {
                var owner = (Belia)Owner;
                var mode = owner._attackMode.Resolve(AttackMode.Any);

                if (owner._attackMode == AttackMode.Any)
                    mode = UnityEngine.Random.Range(0, 3) switch
                    {
                        0 => AttackMode.Slash,
                        1 => AttackMode.CurvedArea,
                        2 => AttackMode.Dash,
                        var i => throw new ArgumentOutOfRangeException(
                            nameof(mode), i, Owner.FormatLogMessage("공격 패턴의 범위는 0 이상 2 이하여야 합니다."))
                    };


                if (!Owner.TryDoAction(new(
                    mode.ToString() + "Attack",
                    result => Complete(result)),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{mode.ToString()} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                    return;
                }

                Blackboard.IsCommitting = true;
                _notification = new MonsterConditionData(
                    MonsterCondition.Attack,
                    mode == AttackMode.Slash || mode == AttackMode.Dash);
                Owner.NotifyCondition(_notification);
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                Blackboard.IsCommitting = false;

                _notification?.Complete();
                _notification = null;
            }
        }
    }
}
