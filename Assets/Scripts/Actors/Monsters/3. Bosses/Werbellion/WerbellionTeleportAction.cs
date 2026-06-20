using Actors.Monsters.Actions;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Verbelion
    {
        private class VerbelionTeleportComponent : MonsterActionComponent
        {
            protected override void OnEnter(object position)
            {
                var Verbelion = (Verbelion)Owner;
                Vector2 targetPosition;

                if (position is Vector3 targetPositionV3)
                    targetPosition = (Vector2)targetPositionV3;
                else if (position is Vector2 targetPositionV2)
                    targetPosition = targetPositionV2;
                else
                    throw new System.ArgumentException(
                        $"VerbelionTeleportComponent: {nameof(position)}은(는) Vector2(3) 형식이어야 하지만" +
                        $"'{position?.GetType().Name ?? "null"}'형식이 입력되었습니다.");

                Verbelion.transform.position = targetPosition;
                Verbelion.transform.localScale = new Vector3
                {
                    x = Verbelion.transform.position.x
                        - Verbelion.DetectedPlayer.transform.position.x
                        > 0 ? -1: 1,
                    y = 1,
                    z = 1,
                } * Verbelion.transform.localScale.z;

                Interrupt(InterruptType.Completed);
            }
        }
    }
}
