using UniEngine.StateMachines.FSM;

public partial class Dokkaebi
{
    private partial class DokkaebiBrain
    {
        private class Dead : Work<DokkaebiBrain>
        {
            protected override void OnEnter(params object[] _)
            {
                Parent.Dokkaebi.TryDie();
            }
        }
    }
}
