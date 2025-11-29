using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Bosses
{
    public partial class Cerberus : Monster<CerberusStats>
    {
        internal class CerberusAttackPhaseBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            public CerberusAttackPhaseBrain()
            {
                HierarchyMode = HierarchyMode.Sequence;
                LoopType = LoopType.Forced;
            }
        }
    }
}
