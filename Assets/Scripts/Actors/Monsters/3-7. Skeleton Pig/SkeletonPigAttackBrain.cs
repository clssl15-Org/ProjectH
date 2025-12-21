using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class SkeletonPig
    {
        private class SkeletonPigAttackBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            public SkeletonPigAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(params object[] _)
            {
                var owner = (SkeletonPig)Owner;
                AttackMode mode;

                if (owner._attackMode == AttackMode.Any)
                    mode = UnityEngine.Random.Range(0, Owner.HP < Owner.StatsInfo.MaxHP ? 3 : 2) switch
                    {
                        0 => AttackMode.DashAttack,
                        1 => AttackMode.StampAttack,
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
                }

                Blackboard.Committing = true;

                // 체력 회복
                if (mode == AttackMode.Roar)
                    Owner.HP += Mathf.FloorToInt(Owner.StatsInfo.MaxHP * owner.StatsInfo.RoarHealingRate);
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                Blackboard.Committing = false;
            }
        }
    }
}
