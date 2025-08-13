using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain
    {
        private class Hit : Work<Alive>
        {
            protected override void Start(params object[] _)
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
