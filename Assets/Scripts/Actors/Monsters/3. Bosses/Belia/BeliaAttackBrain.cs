using System;
using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Belia
    {
        private class BeliaAttackBrain : BTNode<IMonsterInternal, Brains.MonsterBlackboard>
        {
            public BeliaAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(params object[] _)
            {
                var owner = (Belia)Owner;
                AttackMode mode;

                if (owner._attackMode == AttackMode.Any)
                    mode = UnityEngine.Random.Range(0, 3) switch
                    {
                        0 => AttackMode.Slash,
                        1 => AttackMode.CurvedArea,
                        2 => AttackMode.Dash,
                        var i => throw new ArgumentOutOfRangeException(
                            nameof(mode), i, Owner.FormatLogMessage("공격 패턴의 범위는 0 이상 2 이하여야 합니다."))
                    };
                else
                    mode = owner._attackMode;


                if (!Owner.TryDoAction(new(
                    mode.ToString() + "Attack",
                    result => Complete(result)),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{mode.ToString()} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }
            }
        }
    }
}
