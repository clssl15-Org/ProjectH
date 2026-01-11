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
        public Blackbox InteractionPeer { get; }
        public InteractionType Interaction { get; }

        public LogData(string message) : this(null, InteractionType.None, message) { }
        public LogData(Blackbox interactionPeer, InteractionType interaction, string message)
        {
            Message = message;
            Time = DateTime.UtcNow;
            InteractionPeer = interactionPeer;
            Interaction = interaction;
        }

        public override string ToString()
        {
            var time = Time.ToString("HH:mm:ss.fffffff");

            if (InteractionPeer != null)
            {
#pragma warning disable IDE0066
                switch (Interaction)
                {
                    case InteractionType.Self:
                        return $"[{time}] [this <-> this] {Message}";

                    case InteractionType.Exerting:
                        return $"[{time}] [this -> #{InteractionPeer.Id}: {InteractionPeer.OwnerString}] {Message}";

                    case InteractionType.Exerted:
                        return $"[{time}] [#{InteractionPeer.Id}: {InteractionPeer.OwnerString} -> this] {Message}";

                    default:
                        return $"[{time}] [#{InteractionPeer.Id}: {InteractionPeer.OwnerString}] {Message}";
                }
#pragma warning restore
            }

            return $"[{time}] {Message}";
        }
    }
}
