using System;

namespace BlackboxSystem
{
    internal enum InteractionType
    {
        None,
        Self,
        Exerting,
        Exerted
    }

    internal readonly struct LogData
    {
        public string Message { get; }
        public DateTime Time { get; }

        public int ScopeIndex { get; }
        public int ScopeDepth { get; }
        public string MethodName { get; }

        public Blackbox InteractionPeer { get; }
        public InteractionType Interaction { get; }

        public LogData(int scopeIndex, int scopeDepth, string methodName, string message) : this(scopeIndex, scopeDepth, methodName, null, InteractionType.None, message) { }
        public LogData(int scopeIndex, int scopeDepth, string methodName, Blackbox interactionPeer, InteractionType interaction, string message)
        {
            Message = message;
            Time = DateTime.UtcNow;

            ScopeIndex = scopeIndex;
            ScopeDepth = scopeDepth;
            MethodName = methodName;

            InteractionPeer = interactionPeer;
            Interaction = interaction;
        }

        public override string ToString()
        {
            var time = Time.ToString("HH:mm:ss.fffffff");
            var indent = new string(' ', Math.Max(0, (ScopeDepth - 1) * 2));

            var prefix = $"[{time}] {indent}";
            if (!string.IsNullOrEmpty(MethodName)) prefix += $"[{MethodName}] ";

            if (InteractionPeer != null)
            {
#pragma warning disable IDE0066
                switch (Interaction)
                {
                    case InteractionType.Self:
                        return $"{prefix}[this <-> this] {Message}";

                    case InteractionType.Exerting:
                        return $"{prefix}[this -> #{InteractionPeer.Id}: {InteractionPeer.OwnerString}] {Message}";

                    case InteractionType.Exerted:
                        return $"{prefix}[#{InteractionPeer.Id}: {InteractionPeer.OwnerString} -> this] {Message}";

                    default:
                        return $"{prefix}[#{InteractionPeer.Id}: {InteractionPeer.OwnerString}] {Message}";
                }
#pragma warning restore
            }

            return $"{prefix}{Message}";
        }
    }
}
