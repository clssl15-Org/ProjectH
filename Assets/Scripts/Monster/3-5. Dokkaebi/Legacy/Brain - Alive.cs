using UniEngine.StateMachines.FSM;

public partial class Dokkaebi
{
    private partial class DokkaebiBrain
    {
        private class Alive : Work<DokkaebiBrain>
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
