using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        private int _scopeIndex = 0;
        private Stack<string> _scopeStack = new();

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
        }

        public static void ForceResetStaticProperties()
        {
            GlobalId = -1;
            _printed = 0;
        }

        public string Write(string message, string methodName)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("[Blackbox] The message cannot be empty", nameof(message));

            if (Volatile.Read(ref _printed) != 0)
                return message;

            EnqueueLog(new LogData(_scopeIndex, _scopeStack.Count, methodName, message));
            return message;
        }
        public DisposableHandle WriteScope(string message, string methodName)
        {
            if (_scopeStack.Count == 0) _scopeIndex++;
            _scopeStack.Push(methodName);

            Write(message, methodName);
            return new DisposableHandle(this);
        }

        public string Exert(Blackbox other, string message, string methodName)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other), $"[Blackbox] {nameof(other)} cannot be null");

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("[Blackbox] The message cannot be empty", nameof(message));

            if (Volatile.Read(ref _printed) != 0)
                return message;

            if (other != this)
            {
                other.EnqueueLog(new LogData(other._scopeIndex, 0, methodName, this, InteractionType.Exerted, message));
                EnqueueLog(new LogData(_scopeIndex, _scopeStack.Count, methodName, other, InteractionType.Exerting, message));
            }
            else
                EnqueueLog(new LogData(_scopeIndex, _scopeStack.Count, methodName, this, InteractionType.Self, message));

            return message;
        }
        public DisposableHandle ExertScope(Blackbox other, string message, string methodName)
        {
            if (_scopeStack.Count == 0) _scopeIndex++;
            _scopeStack.Push(methodName);

            Exert(other, message, methodName);
            return new DisposableHandle(this);
        }

        /// <summary>
        /// This method is called by DisposableHandle.
        /// </summary>
        internal void PopScope()
        {
            if (_scopeStack.Count > 0)
            {
                _scopeStack.Pop();
                _scopeIndex++;
            }
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

            sb.AppendLine($"========= #{Id}: {OwnerString} ({description}) =========");

            int currentScopeIndex = 0;

            int tryDequeueCount = 0;
            while (tryDequeueCount < MaxTryCount)
            {
                if (_logs.TryDequeue(out var logData))
                {
                    if (logData.InteractionPeer != null && logData.InteractionPeer != this)
                        relatedBlackboxes.Add(logData.InteractionPeer);

                    if (currentScopeIndex != logData.ScopeIndex)
                    {
                        sb.AppendLine(new string('-', 30));
                        currentScopeIndex = logData.ScopeIndex;
                    }

                    var message = logData.ToString();

                    if (logData.Interaction == InteractionType.Exerted)
                        message = $"-> {message}";

                    sb.AppendLine(message);
                    tryDequeueCount = 0;
                }
                else
                    tryDequeueCount++;
            }

            if (currentScopeIndex != 0)
                sb.AppendLine(new string('-', 30));


            if (currentDepth < maxDepth)
            {
                currentDepth += 1;

                foreach (var subject in relatedBlackboxes)
                {
                    if (history.Contains(subject))
                        continue;

                    sb.AppendLine();
                    sb.AppendLine();
                    sb.Append(subject.Print(currentDepth, maxDepth, this, history));
                }
            }

            return sb.ToString();
        }
    }
}
