using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Brains
{
    internal class PlayerDetected : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        private bool _wasDetected = false;

        public PlayerDetected()
        {
            AbortPolicies = AbortPolicies.LowerPriority | AbortPolicies.Self;
            HierarchyMode = HierarchyMode.Sequence;
            LoopType = LoopType.Forced;
        }

        public override bool CheckCondition()
        {
            if (Blackboard.IsCommitting)
                return true;

            var player = Owner.DetectedPlayer;
            if (!player)
            {
                _wasDetected = false;
                return false;
            }

            var playerPlatform = player.GetComponent<IPlayer>().CurrentPlatform;
            return playerPlatform >= 0 && playerPlatform == Owner.CurrentPlatform;
        }

        protected override void OnOpen(params object[] _)
        {
            if (!_wasDetected)
            {
                _wasDetected = true;

                Owner.NotifyCondition(
                    new MonsterConditionData(MonsterCondition.PlayerDetected));
            }
        }
    }
}
