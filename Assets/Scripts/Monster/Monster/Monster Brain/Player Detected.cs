using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    public class PlayerDetected : BTNode<Monster, MonsterBlackboard>
    {
        public PlayerDetected()
        {
            SelectionOption = SelectionOptions.LowerPriority | SelectionOptions.Self;
            HierarchyMode = HierarchyMode.Sequence;
            LoopType = LoopType.Forced;
        }

        public override bool CheckCondition()
        {
            if (Blackboard.Committing)
                return true;

            var player = Owner.DetectedPlayer;
            if (!player) return false;

            var playerPlatform = player.GetComponent<TestPlayer>().CurrentPlatform;
            return playerPlatform >= 0 && playerPlatform == Owner.BelongingPlatform;
        }
    }
}
