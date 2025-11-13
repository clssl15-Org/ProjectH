namespace Infrastructure.StateMachines.FSM
{
    public class Work<TParent> : Work where TParent : Work
    {
        public Work(string name = null) : base(name) { }
        protected new TParent Parent => (TParent)hierarchy.Parent;
    }
}
