using Actors.Monsters.Actions;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Werbellion
    {
        private class WerbellionTeleportComponent : MonsterActionComponent
        {
            protected override void OnEnter(object position)
            {
                var werbellion = (Werbellion)Owner;
                Vector2 targetPosition;

                if (position is Vector3 targetPositionV3)
                    targetPosition = (Vector2)targetPositionV3;
                else if (position is Vector2 targetPositionV2)
                    targetPosition = targetPositionV2;
                else
                    throw new System.ArgumentException(
                        $"WerbellionTeleportComponent: {nameof(position)}은(는) Vector2(3) 형식이어야 하지만" +
                        $"'{position?.GetType().Name ?? "null"}'형식이 입력되었습니다.");

                werbellion.transform.position = targetPosition;
                werbellion.transform.localScale = new Vector3(
                    werbellion.transform.position.x - werbellion._targetPlayer.transform.position.x
                    > 0 ? -1 : 1 
                    , 1, 1);

                Interrupt(InterruptType.Completed);
            }
        }
    }
}
