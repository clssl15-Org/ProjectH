using UniEngine.StateMachines.FSM;

public partial class Dokkaebi
{
    private partial class DokkaebiBrain : Work
    {
        // Internal
        protected Dokkaebi Dokkaebi { get; private set; }

        
        // Content
        public DokkaebiBrain(Dokkaebi dokkaebi)
        {
            Dokkaebi = dokkaebi;

            AddChild(new Alive(), true);
            AddChild(new Dead());
        }


        // Forwarding
        public void TakeDamage(int damage)
        {
            if (TryGetCurrentChild<Alive>(out var current))
                current.TakeDamage(damage);
        }
    }
}
