using System;

namespace BlackboxSystem
{
    internal enum ScopeType
    {
        None,
        Open,
        Close
    }

    internal readonly struct LogData
    {
        public Blackbox Owner { get; }

        public string Message { get; }
        public DateTime Time { get; }

        public int ScopeDepth { get; }
        public string MethodName { get; }
        public ScopeType ScopeType { get; }

        public Blackbox ExertedBy { get; }
        public Blackbox ExertingTo { get; }
        public long InteractionId { get; }

        private const int IndentCount = 4;

        public LogData(Blackbox owner, int scopeDepth, string message, string methodName, ScopeType scopeType, long interactionId = -1)
            : this(owner, scopeDepth, message, methodName, scopeType, null, null, interactionId) { }
        public LogData(Blackbox owner, int scopeDepth, string message, string methodName, ScopeType scopeType, Blackbox exertedBy, Blackbox exertingTo, long interactionId = -1)
        {
            Owner = owner;

            Message = message;
            Time = DateTime.UtcNow;

            ScopeDepth = scopeDepth >= 0 ? scopeDepth : 0;
            MethodName = methodName;
            ScopeType = scopeType;

            ExertedBy = exertedBy;
            ExertingTo = exertingTo;
            InteractionId = interactionId;
        }

        public override string ToString()
        {
            var time = Time.ToString("HH:mm:ss.fffffff");
            var indent = "";
            for (int i = 0; i < Math.Max(0, ScopeDepth); i++)
                indent += $"|{new string(' ', Math.Max(0, IndentCount - 1))}";

            var prefix = $"[{time}] {indent}";

            if (!string.IsNullOrEmpty(MethodName))
                prefix += ScopeType switch
                {
                    ScopeType.Open => $"<{MethodName}> ",
                    ScopeType.Close => $"</{MethodName}> ",
                    _ => $"[{MethodName}] ",
                };

            if (ExertedBy != null && ExertingTo != null)
                return $"{prefix}{Message} (#{ExertedBy.Id}: {ExertedBy.OwnerString} {Arrow(true, InteractionId)} #{Owner.Id}: this {Arrow(true, InteractionId)} #{ExertingTo.Id}: {ExertingTo.OwnerString} )";
            if (ExertedBy != null)
                return $"{prefix}{Message} (#{Owner.Id}: this {Arrow(false, InteractionId)} #{ExertedBy.Id}: {ExertedBy.OwnerString})";
            if (ExertingTo != null)
                return $"{prefix}{Message} (#{Owner.Id}: this {Arrow(true, InteractionId)} #{ExertingTo.Id}: {ExertingTo.OwnerString})";

            return $"{prefix}{Message}";


            string Arrow(bool right, long interactionId)
            {
                if (interactionId >= 0)
                    return right ? $"-[{interactionId}]->" : $"<-[{interactionId}]-";
                else
                    return right ? "->" : "<-";
            }
        }
    }
}
