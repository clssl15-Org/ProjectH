using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Stage3Bosses
{
    internal class Awaken : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Content
        public Awaken()
        {
            AbortPolicies = AbortPolicies.Self;
            HierarchyMode = HierarchyMode.Sequence;
            LoopType = LoopType.Forced;
        }

        public override bool CheckCondition() =>
            (bool)Blackboard.Properties[ITwinBoss.IsAwake];
    }
}
