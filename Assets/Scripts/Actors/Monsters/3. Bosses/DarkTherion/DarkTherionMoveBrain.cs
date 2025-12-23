using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class DarkTherion
    {
        private class DarkTherionMoveBrain : BTNode<IMonsterInternal, Brains.MonsterBlackboard>
        {
            // Internal
            private DarkTherion DarkTherion => (DarkTherion)Owner;
            private Rigidbody2D Rigidbody => DarkTherion.Rigidbody;

            private int _before = -1;
            private Vector2 _targetPoint;


            // Content
            public DarkTherionMoveBrain() : base(name: MonsterActionType.Walk.ToString()) { }

            public override bool CheckCondition()
            {
                if (DarkTherion._movePoints.Length == 0)
                {
                    Debug.LogWarning(DarkTherion.FormatLogMessage(
                        $"{nameof(_movePoints)}은(는) 하나 이상의 지점을 포함해야 합니다."));
                    return false;
                }

                return true;
            }

            protected override void OnOpen(params object[] _)
            {
                if (!Owner.TryDoAction(
                    new(Name),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{Name} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }

                int next = 0;
                if (DarkTherion._movePoints.Length > 1)
                {
                    do
                    {
                        next = Random.Range(0, DarkTherion._movePoints.Length);
                    } while (next == _before);
                }

                _targetPoint = DarkTherion
                    ._movePoints[next]
                    .position;

                _before = next;
            }
            
            protected override void OnTick()
            {
                var toTarget = _targetPoint - Rigidbody.position;
                var distance = toTarget.magnitude;

                if (distance <= DarkTherion._arriveDistanceTolerance)
                {
                    Rigidbody.position = _targetPoint;
                    Rigidbody.velocity = Vector2.zero;

                    Complete();
                    return;
                }

                var dir = toTarget / distance;

                var currentSpeed = Vector2.Dot(Rigidbody.velocity, dir);
                if (currentSpeed < 0f) currentSpeed = 0f;

                var stoppingDistance = (currentSpeed * currentSpeed) / (2f * DarkTherion._deceleration);

                var accel = stoppingDistance >= distance
                    ? -DarkTherion._deceleration
                    : DarkTherion._acceleration;

                var newSpeed = currentSpeed + accel * Time.fixedDeltaTime;
                newSpeed = Mathf.Clamp(newSpeed, 0f, DarkTherion.StatsInfo.MoveSpeed);

                Rigidbody.velocity = dir * newSpeed;
            }
        }
    }
}
