namespace UniEngine.StateMachines.FSM
{
    public class Work<TParent> : Work where TParent : Work
    {
        public Work(string name = null) : base(name) { }
        protected TParent Parent => (TParent)hierarchy.Parent;
    }
}
