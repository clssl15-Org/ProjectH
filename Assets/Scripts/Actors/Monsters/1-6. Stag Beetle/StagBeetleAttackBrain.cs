using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class StagBeetle
    {
        private class StagBeetleAttackBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            // Internal
            private MonsterConditionData _notification;


            // Content
            public StagBeetleAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(params object[] _)
            {
                var owner = (StagBeetle)Owner;
                AttackMode mode;

                if (owner._attackMode == AttackMode.Any)
                    mode = UnityEngine.Random.Range(0, Owner.HP < Owner.StatsInfo.MaxHP ? 3 : 2) switch
                    {
                        0 => AttackMode.RollAttack,
                        1 => AttackMode.SpikeAttack,
                        2 => AttackMode.Roar,
                        var i => throw new ArgumentOutOfRangeException(
                            nameof(mode), i, Owner.FormatLogMessage($"공격 패턴의 범위는 0 이상 2 이하여야 합니다."))
                    };
                else
                    mode = owner._attackMode;

                if (!Owner.TryDoAction(new(mode.ToString(), result => Complete(result)), out var reason))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{mode.ToString()} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                    return;
                }


                Blackboard.Committing = true;

                if (mode == AttackMode.Roar)
                    Owner.HP += Mathf.FloorToInt(Owner.StatsInfo.MaxHP * owner.StatsInfo.RoarHealingRate);
                else
                {
                    _notification = new MonsterConditionData(MonsterCondition.Attack, true);
                    Owner.NotifyCondition(_notification);
                }
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                Blackboard.Committing = false;

                _notification?.Complete();
                _notification = null;
            }
        }
    }
}
