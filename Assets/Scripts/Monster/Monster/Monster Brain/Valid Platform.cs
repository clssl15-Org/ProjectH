using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class ValidPlatform : BTNode<Monster, MonsterBlackboard>
    {
        public ValidPlatform()
        {
            SelectionOption = SelectionOptions.Self;
            HierarchyMode = HierarchyMode.Selector;
            LoopType = LoopType.Forced;
        }


        public override bool CheckCondition()
        {
            if (Blackboard.Committing)
                return true;

            return Owner.PlatformDetector.CheckPlatform(
                Direction.Center, Owner.BelongingPlatform, out _);
        }
    }
}
