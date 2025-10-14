using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal class PlayerDetected : BTNode<IMonster, MonsterBlackboard>
    {
        public PlayerDetected()
        {
            AbortPolicy = AbortPolicies.LowerPriority | AbortPolicies.Self;
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
