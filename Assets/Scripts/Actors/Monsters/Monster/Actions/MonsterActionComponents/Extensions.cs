namespace Actors.Monsters.Actions
{
    public static class MonsterActionComponentExtensions
    {
        public static T AssignTo<T>(this T component, out T self)
        {
            self = component;
            return component;
        }
    }
}
