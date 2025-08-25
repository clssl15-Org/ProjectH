using System.Collections.Generic;

public class MonsterBlackboard
{
    // Front
    public bool Committing { get; set; } = false;
    public Dictionary<object, object> Properties { get; } = new();
}
