using System.Collections.Generic;

namespace Actors.Monsters.Brains
{
    public class MonsterBlackboard
    {
        // Front
        public bool Committing { get; set; } = false;
        public bool Moved { get; set; }
        public Dictionary<object, object> Properties { get; } = new();
    }
}
