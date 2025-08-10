using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain
    {
        private class SetPlatform : Work<Alive>
        {
            // Internal
            public Dokkaebi Dokkaebi => Parent.Dokkaebi;


            // Content
            protected override void Update()
            {
                if (Dokkaebi.PlatformDetector.TryGetCurrentPlatformId(out var platformId))
                {
                    Dokkaebi.BelongingPlatform = platformId;
                    Parent.SetNext(typeof(OnValidPlatform));
                }
            }
        }
    }
}
