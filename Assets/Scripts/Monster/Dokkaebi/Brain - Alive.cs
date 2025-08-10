using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain
    {
        private class Alive : Work<Brain>
        {
            // Internal
            public Dokkaebi Dokkaebi => Parent.Dokkaebi;


            // Content
            public Alive()
            {
                AddChild(new SetPlatform(), true);
                AddChild(new OnValidPlatform());
            }

            public bool CheckPlatform(Direction direction, out int detectedPlatformId)
            {
                return Dokkaebi.PlatformDetector.CheckPlatform(
                    direction, Dokkaebi.BelongingPlatform, out detectedPlatformId);
            }

            public void TakeDamage(int damage)
            {
                var hp = Dokkaebi.HP - damage;

                if (Dokkaebi.HP <= 0)
                {
                    Dokkaebi.HP = 0;
                    Parent.SetNext(typeof(Dead));
                }
                else
                {
                    Dokkaebi.HP = hp;
                }
            }
        }
    }
}
