using Actors.Monsters.Brains;
using Infrastructure;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters
{
    public partial class FireMonster
    {
        private class LookPlayerBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            protected override void OnOpen(object[] _)
            {
                Owner.Direction =
                    (Owner.DetectedPlayer.transform.position
                    - Owner.transform.position)
                    .ToDirection();

                Complete(true);
            }
        }
    }
}
