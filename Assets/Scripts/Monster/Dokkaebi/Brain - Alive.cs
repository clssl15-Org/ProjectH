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
                AddChild(new Hit());
            }

            public bool CheckPlatform(Direction direction, out int detectedPlatformId)
            {
                return Dokkaebi.PlatformDetector.CheckPlatform(
                    direction, Dokkaebi.BelongingPlatform, out detectedPlatformId);
            }

            public void TakeDamage(int damage)
            {
                if (TryGetCurrentChild<Hit>(out _))
                    return;


                var hp = Dokkaebi.HP - damage;

                if (hp <= 0)
                {
                    Dokkaebi.HP = 0;
                    Parent.SetNext<Dead>();
                }
                else
                {
                    Dokkaebi.HP = hp;
                    SetNext<Hit>();
                }
            }
        }
    }
}
