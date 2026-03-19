using System.Collections.Generic;

namespace Actors.Monsters.Brains
{
    public class MonsterBlackboard
    {
        // Front
        public bool IsCommitting { get; set; } = false;
        public bool IsMoved { get; set; }
        public Dictionary<object, object> Properties { get; } = new();
    }
}
