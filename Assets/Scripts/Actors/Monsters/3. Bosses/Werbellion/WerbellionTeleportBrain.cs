using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Verbelion
    {
        private class VerbelionTeleportBrain : BTNode<IMonsterInternal, Brains.MonsterBlackboard>
        {
            // Internal
            private Verbelion Verbelion => (Verbelion)Owner;
            private Rigidbody2D Rigidbody => Verbelion.Rigidbody;

            private int _before = -1;


            // Content
            public VerbelionTeleportBrain() : base(name: "Teleport") { }

            public override bool CheckCondition()
            {
                if (Verbelion._groundPoints.Length == 0)
                {
                    Debug.LogWarning(Verbelion.FormatLogMessage(
                        $"{nameof(_groundPoints)}은(는) 하나 이상의 지점을 포함해야 합니다."));
                    return false;
                }

                return true;
            }

            protected override void OnOpen(params object[] _)
            {
                var teleportPosition = (Vector2)Verbelion
                    ._groundPoints
                    .GetRandomItem(ref _before)
                    .position;


                if (!Owner.TryDoAction(new(
                    Name,
                    Callback: result => Complete(result),
                    Inputs: new object[] { null, null, teleportPosition }),
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
