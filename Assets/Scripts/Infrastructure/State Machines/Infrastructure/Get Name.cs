using System;

namespace Infrastructure.StateMachines
{
    public static partial class Tools
    {
        public static string GetName(object source) => source switch
        {
            null => throw new ArgumentNullException(nameof(source), $"Name source cannot be null."),
            string name when string.IsNullOrWhiteSpace(name) => throw new ArgumentException($"Name cannot be empty.", nameof(source)),
            string name => name,
            Type type => type.Name,
            _ => source.ToString()
        };

    }
}

