using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BlackboxSystem
{
    internal class Blackbox
    {
        // Front
        public object Owner
        {
            get
            {
                if (_strongOwner != null)
                    return _strongOwner;

                if (_weakOwner?.TryGetTarget(out var owner) ?? false)
                    return owner;

                return null;
            }
        }

        public string OwnerString
        {
            get
            {
                var owner = Owner;

                if (owner != null)
                    return owner.ToString();

                if (!string.IsNullOrEmpty(_ownerDescription))
                    return $"{_ownerDescription} (Reference Lost)";

                return "null";
            }
        }

        public long Id { get; }


        // Internal
        private static long GlobalId = -1;
        private const int MaxTryCount = 10;

        private object _strongOwner;
        private WeakReference<object> _weakOwner;
        private string _ownerDescription;

        private ConcurrentQueue<LogData> _logs = new();

        private long _logCount = 0;
        private static int _printed = 0;


        // Content
        public Blackbox(object owner, bool strongReference)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner), "[Blackbox] Owner must not be null");

            if (strongReference)
                _strongOwner = owner;
            else
                _weakOwner = new(owner);

            Id = Interlocked.Increment(ref GlobalId);
            _ownerDescription = owner.ToString();

            Write("Created");
        }

        public static void ForceResetStaticProperties()
        {
            GlobalId = -1;
            _printed = 0;
        }

        public string Write(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("[Blackbox] The message cannot be empty", nameof(message));

            if (Volatile.Read(ref _printed) != 0)
                return message;

            EnqueueLog(new LogData(message));
            return message;
        }

        public string Exert(Blackbox other, string message)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other), $"[Blackbox] {nameof(other)} cannot be null");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("[Blackbox] The message cannot be empty", nameof(message));

            if (Volatile.Read(ref _printed) != 0)
                return message;

            if (other != this)
            {
                other.EnqueueLog(new LogData(this, InteractionType.Exerted, message));
                EnqueueLog(new LogData(other, InteractionType.Exerting, message));
            }
            else
                EnqueueLog(new LogData(this, InteractionType.Self, message));

            return message;
        }

        private void EnqueueLog(LogData logData)
        {
            if (Volatile.Read(ref _printed) != 0)
                return;

            _logs.Enqueue(logData);
            Interlocked.Increment(ref _logCount);

            if (Infrastructure.MaxLogCount < 0)
                return;

            int tryDequeueCount = 0;
            while (Volatile.Read(ref _logCount) > Infrastructure.MaxLogCount)
            {
                if (_logs.TryDequeue(out _))
                {
                    tryDequeueCount = 0;
                    Interlocked.Decrement(ref _logCount);
                }
                else
                {
                    tryDequeueCount++;
                    if (tryDequeueCount > MaxTryCount) break;
                }
            }
        }

        public bool TryPrint(int recursionDepth, out string result)
        {
            if (Interlocked.CompareExchange(ref _printed, 1, 0) != 0)
            {
                result = string.Empty;
                return false;
            }

            result = Print(
                currentDepth: 0,
                maxDepth: recursionDepth >= 0 ? recursionDepth : int.MaxValue,
                before: null,
                history: new());

            return true;
        }
        private string Print(int currentDepth, int maxDepth, Blackbox before, HashSet<Blackbox> history)
        {
            history.Add(this);

            var sb = new StringBuilder();
            var relatedBlackboxes = new HashSet<Blackbox>();

            var description = $"Depth = {currentDepth}";
            if (before != null) description += $" | From = #{before.Id}: {before.OwnerString}";

            sb.AppendLine($"========= {OwnerString} (Id = #{Id} | {description}) =========");


            int tryDequeueCount = 0;
            while (tryDequeueCount < MaxTryCount)
            {
                if (_logs.TryDequeue(out var logData))
                {
                    if (logData.InteractionPeer != null && logData.InteractionPeer != this)
                        relatedBlackboxes.Add(logData.InteractionPeer);

                    var message = logData.ToString();

                    if (before != null && logData.InteractionPeer == before)
                        message = $"-> {message}";

                    sb.AppendLine(message);
                    tryDequeueCount = 0;
                }
                else
                    tryDequeueCount++;
            }

            if (currentDepth < maxDepth)
            {
                currentDepth += 1;

                foreach (var subject in relatedBlackboxes)
                {
                    if (history.Contains(subject))
                        continue;

                    sb.AppendLine();
                    sb.Append(subject.Print(currentDepth, maxDepth, this, history));
                }
            }

            return sb.ToString();
        }
    }
}
