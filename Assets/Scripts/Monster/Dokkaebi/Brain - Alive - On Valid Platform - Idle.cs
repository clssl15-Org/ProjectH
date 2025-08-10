using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain
    {
        private class Idle : Work<OnValidPlatform>
        {
            // Internal
            public Dokkaebi Dokkaebi => Parent.Dokkaebi;


            // Content
            protected override void Update()
            {

            }
        }
    }
}
