using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain : Work
    {
        // Internal
        protected Dokkaebi Dokkaebi { get; private set; }

        
        // Content
        public Brain(Dokkaebi dokkaebi)
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
