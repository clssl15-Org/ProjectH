using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class SkeletonPig
    {
        private class SkeletonPigAttackBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            // Internal
            private MonsterConditionData _notification;


            // Content
            public SkeletonPigAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(params object[] _)
            {
                var owner = (SkeletonPig)Owner;
                var mode = owner._attackMode.Resolve(AttackMode.Any);

                if (owner._attackMode == AttackMode.Any)
                    mode = UnityEngine.Random.Range(0, Owner.HP < Owner.StatsInfo.MaxHP ? 3 : 2) switch
                    {
                        0 => AttackMode.DashAttack,
                        1 => AttackMode.StampAttack,
                        2 => AttackMode.Roar,
                        var i => throw new ArgumentOutOfRangeException(
                            nameof(mode), i, Owner.FormatLogMessage($"공격 패턴의 범위는 0 이상 2 이하여야 합니다."))
                    };

                _notification = new(
                    mode != AttackMode.Roar ? MonsterCondition.Attack : MonsterCondition.Heal,
                    new MonsterAttackData(mode.ToString(), mode == AttackMode.DashAttack));

                if (!Owner.TryDoAction(new MonsterActionPlayInfo(
                     Name: mode.ToString(),
                     Callback: result => Complete(result),
                     Inputs: new object[]
                     {
                         null,
                         new AttackWithWeapon.Payload(MonsterConditionData: _notification)
                     }),
                     out var reason))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{mode.ToString()} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                    return;
                }

                Blackboard.IsCommitting = true;

                if (mode == AttackMode.Roar)
                {
                    Owner.HP += Mathf.FloorToInt(Owner.StatsInfo.MaxHP * owner.StatsInfo.RoarHealingRate);
                    Owner.NotifyCondition(_notification);

                    ((MonsterAttackData)_notification.Payload).OnExecuting();
                    _notification.Complete();
                }
                else
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
