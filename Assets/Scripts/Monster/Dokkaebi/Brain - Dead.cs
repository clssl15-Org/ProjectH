using Infrastructure;

public partial class Dokkaebi
{
    private partial class Brain
    {
        private class Dead : Work<Brain>
        {
            protected override void OnEnter(params object[] _)
            {
                Parent.Dokkaebi.TryDie();
            }
        }
    }
}
