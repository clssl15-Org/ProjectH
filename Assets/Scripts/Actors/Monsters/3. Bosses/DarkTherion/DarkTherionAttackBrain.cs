using System;
using Actors.Monsters.Actions;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class DarkTherion
    {
        private class DarkTherionAttackBrain : BTNode<IMonsterInternal, Brains.MonsterBlackboard>
        {
            // Internal
            private MonsterConditionData _notification;


            // Content
            public DarkTherionAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(params object[] _)
            {
                var owner = (DarkTherion)Owner;
                var mode = owner._attackMode.Resolve(AttackMode.Any);

                if (owner._attackMode == AttackMode.Any)
                    mode = UnityEngine.Random.Range(0, 3) switch
                    {
                        0 => AttackMode.Projectile,
                        1 => AttackMode.Bullet,
                        2 => AttackMode.Spike,
                        var i => throw new ArgumentOutOfRangeException(
                            nameof(mode), i, Owner.FormatLogMessage("공격 패턴의 범위는 0 이상 2 이하여야 합니다."))
                    };


                if (!Owner.TryDoAction(new(
                    mode.ToString() + "Attack",
                    result => Complete(result),
                    Inputs: new object[] { null, (Func<Vector2>)(() => owner.DetectedPlayer.transform.position) }),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{mode.ToString()} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                    return;
                }

                _notification = new MonsterConditionData(MonsterCondition.Attack, false);
                Owner.NotifyCondition(_notification);
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                _notification?.Complete();
                _notification = null;
            }
        }
    }
}
