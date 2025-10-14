using UniEngine.StateMachines.BT;

public partial class FireMonster
{
    private class LookPlayerBrain : BTNode<IMonster, MonsterBlackboard>
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
