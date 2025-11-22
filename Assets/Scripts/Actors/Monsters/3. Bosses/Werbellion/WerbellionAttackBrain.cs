using System;
using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public partial class Werbellion
    {
        private class WerbellionAttackBrain : BTNode<IMonsterInternal, Brains.MonsterBlackboard>
        {
            public WerbellionAttackBrain(int index)
                : base(name: $"{MonsterActionType.Attack} {index}") { }

            protected override void OnOpen(params object[] _)
            {
                var owner = (Werbellion)Owner;
                AttackMode mode;

                if (owner._attackMode == AttackMode.Any)
                    mode = (AttackMode)UnityEngine.Random.Range(
                        minInclusive: 1, // Any 제외
                        maxExclusive: Enum.GetValues(typeof(AttackMode)).Length);
                else
                    mode = owner._attackMode;


                if (!Owner.TryDoAction(new(
                    mode.ToString() + "Attack",
                    Inputs: new object[] { null, (Func<Vector2>)(() => owner._targetPlayer.transform.position) },
                    Callback: result => Complete(result)),
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
