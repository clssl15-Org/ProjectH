using Actors.Monsters.Actions;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public partial class Werbellion
    {
        private class WerbellionTeleportComponent : MonsterActionComponent
        {
            protected override void OnEnter(float _, object position)
            {
                var werbellion = (Werbellion)Owner;

                if (position is not Vector2 targetPosition)
                    throw new System.ArgumentException(
                        $"WerbellionTeleportComponent: {nameof(position)}은(는) Vector2 형식이어야 하지만" +
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
