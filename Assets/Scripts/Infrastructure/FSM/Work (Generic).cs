namespace Infrastructure
{
    public class Work<TParent> : Work where TParent : Work
    {
        protected TParent Parent => (TParent)hierarchyManager.Parent;
    }
}
