using UniEngine.StateMachines.FSM;

public partial class Dokkaebi
{
    private partial class DokkaebiBrain
    {
        private class Hit : Work<Alive>
        {
            protected override void OnEnter(params object[] _)
            {
                var damaging = Parent.Dokkaebi.TryGetDamage(() =>
                {
                    if (Active)
                        Parent.SetNext<SetPlatform>();
                });

                if (!damaging)
                    Parent.SetNext<SetPlatform>();
            }
        }
    }
}
