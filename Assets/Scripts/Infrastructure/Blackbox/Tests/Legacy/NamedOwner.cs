namespace BlackboxSystem.Tests
{
    internal class NamedOwner
    {
        public string Name { get; }
        public NamedOwner(string name) => Name = name;
        public override string ToString() => Name;
    }
}
