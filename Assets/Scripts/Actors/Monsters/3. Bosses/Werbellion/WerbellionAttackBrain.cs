using System;
using System.Collections.Generic;
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
            private readonly List<AttackMode> _avoidNext = new();
            private Action _callback;
            private MonsterConditionData _notification;


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

            private bool TryDoNextAttack(out Action nextAction)
            {
                _notification?.Complete();
                _notification = null;

                _phase++;
                if (_phase >= 3)
                {
                    _phase = -1;
                     nextAction = null;
                    return false;
                }

                var attackmode = Werbellion._attackMode.Resolve(AttackMode.Any);
                if (attackmode == AttackMode.Any)
                {
                    do
                    {
                        attackmode = (AttackMode)UnityEngine.Random.Range(
                            minInclusive: 1, // Any 제외
                            maxExclusive: Enum.GetValues(typeof(AttackMode)).Length);
                    } while (_avoidNext.Contains(attackmode));
                }

                _avoidNext.Clear();


                var attackName = attackmode.ToString() + "Attack";
                if (attackmode == AttackMode.Portal)
                {
                    _avoidNext.Add(attackmode);
                    nextAction = () => AirAttack(attackName);
                }
                else
                    nextAction = () => GroundAttack(attackName);

                var isRangedAttack =
                    attackmode == AttackMode.Punch
                    || attackmode == AttackMode.StraightArea
                    || attackmode == AttackMode.Stun;

                _notification = new MonsterConditionData(
                    MonsterCondition.Attack,
                    new MonsterAttackData(attackName, isRangedAttack));

                Owner.NotifyCondition(_notification);
                return true;
            }

            private void GroundAttack(string name)
            {
                if (!Owner.TryDoAction(new(
                    name,
                    Inputs: new object[]
                    {
                        null,
                        (Func<Vector2>)(() => Werbellion.DetectedPlayer.transform.position)
                    },
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
                        $"{name} 행동에 실패하였기 때문에 '{Name}' 상태로 진입할 수 없습니다.\n{reason}"));

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
                        // Teleport In
                        null,
                        null,
                        airPos,
                        null,
                        null,

                        // Attack
                        null,
                        (Func<Vector2>)(() => Werbellion._targetPlayer.transform.position),

                        // Teleport Out
                        null,
                        null,
                        groundPos,
                        null,
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
