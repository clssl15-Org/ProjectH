using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Bosses
{
    internal class Awaken : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Internal
        private readonly string _isAwakenKey;

        // Content
        public Awaken(string isAwakenKey)
        {
            _isAwakenKey = isAwakenKey;

            HierarchyMode = HierarchyMode.Sequence;
            LoopType = LoopType.Forced;
        }

        public override bool CheckCondition() =>
            (bool)Blackboard.Properties[_isAwakenKey];
    }
}
