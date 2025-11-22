using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public partial class Werbellion
    {
        private class WerbellionTeleportBrain : BTNode<IMonsterInternal, Brains.MonsterBlackboard>
        {
            // Internal
            private Werbellion Werbellion => (Werbellion)Owner;
            private Rigidbody2D Rigidbody => Werbellion.Rigidbody;

            private int _before = -1;


            // Content
            public WerbellionTeleportBrain() : base(name: "Teleport") { }

            public override bool CheckCondition()
            {
                if (Werbellion._movePoints.Length == 0)
                {
                    Debug.LogWarning(Werbellion.FormatLogMessage(
                        $"{nameof(_movePoints)}은(는) 하나 이상의 지점을 포함해야 합니다."));
                    return false;
                }

                return true;
            }

            protected override void OnOpen(params object[] _)
            {
                int next = 0;
                if (Werbellion._movePoints.Length > 1)
                {
                    do
                    {
                        next = Random.Range(0, Werbellion._movePoints.Length);
                    } while (next == _before);
                }

                var teleportPosition = (Vector2)Werbellion._movePoints[next].position;
                _before = next;


                if (!Owner.TryDoAction(new(
                    Name,
                    Callback: result => Complete(result),
                    Inputs: new object[] { null, teleportPosition }),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{Name} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }
            }
        }
    }
}
