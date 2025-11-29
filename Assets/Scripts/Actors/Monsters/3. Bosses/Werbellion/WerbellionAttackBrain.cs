using System;
using Actors.Monsters.Actions;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Werbellion
    {
        private class WerbellionAttackBrain : BTNode<IMonsterInternal, Brains.MonsterBlackboard>
        {
            private Werbellion Werbellion => (Werbellion)Owner;
            private int _phase = -1;
            private int _beforeAirPos = -1;
            private Action _callback;


            public WerbellionAttackBrain()
                : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(params object[] _)
            {
                if (!TryDoNextAttack(out _callback))
                    Complete(false);
            }

            protected override void OnTick()
            {
                if (_callback != null)
                {
                    var callback = _callback;
                    _callback = null;

                    callback();
                }
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                _phase = -1;
                _beforeAirPos = -1;
                _callback = null;
            }

            private bool TryDoNextAttack(out Action doNextAction)
            {
                _phase++;
                if (_phase >= 3)
                {
                    _phase = -1;
                     doNextAction = null;
                    return false;
                }

                AttackMode mode;

                if (Werbellion._attackMode == AttackMode.Any)
                    mode = (AttackMode)UnityEngine.Random.Range(
                        minInclusive: 1, // Any 제외
                        maxExclusive: Enum.GetValues(typeof(AttackMode)).Length);
                else
                    mode = Werbellion._attackMode;

                var attackName = mode.ToString() + "Attack";

                if (mode == AttackMode.Spike)
                    doNextAction = () => AirAttack(attackName);
                else
                    doNextAction = () => GroundAttack(attackName);

                return true;
            }

            private void GroundAttack(string name)
            {
                if (!Owner.TryDoAction(new(
                    name,
                    Inputs: new object[] { null, (Func<Vector2>)(() => Werbellion._targetPlayer.transform.position) },
                    Callback: result =>
                    {
                        if (!result)
                        {
                            Complete(false);
                            return;
                        }

                        if (!TryDoNextAttack(out _callback))
                            Complete(true);
                    }),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{name} 행동에 실패하였기 때문에 {Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }
            }

            private void AirAttack(string name)
            {
                var groundPos = Werbellion.transform.position;
                var airPos = (Vector2)Werbellion._airPoints.GetRandomItem(ref _beforeAirPos).position;

                if (!Owner.TryDoAction(new(
                    name,
                    Inputs: new object[]
                    {
                        null,
                        airPos,
                        null,
                        null,
                        (Func<Vector2>)(() => Werbellion._targetPlayer.transform.position),
                        null,
                        null,
                        null,
                        null,
                        groundPos,
                        null,
                    },
                    Callback: result =>
                    {
                        Blackboard.Properties[IsOnAir] = false;

                        if (!result)
                        {
                            Complete(false);
                            return;
                        }

                        if (!TryDoNextAttack(out _callback))
                            Complete(true);
                    }),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{name} 행동에 실패하였기 때문에 {Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                    return;
                }

                Blackboard.Properties[IsOnAir] = true;
            }
        }
    }
}
