using System;
using System.Collections.Generic;
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
                if (_strongOwner != null) return _strongOwner;
                if (_weakOwner?.TryGetTarget(out var owner) ?? false) return owner;
                return null;
            }
        }

        public string OwnerString
        {
            get
            {
                try
                {
                    var owner = Owner;

                    if (owner != null)
                        return owner.ToString() ?? "null";

                    if (!string.IsNullOrEmpty(_ownerDescription))
                        return $"{_ownerDescription} (Reference Lost)";

                    return "null";
                }
                catch (Exception ex)
                {
                    if (!string.IsNullOrEmpty(_ownerDescription))
                        return $"{_ownerDescription} (Fallback: {ex.GetType().Name})";

                    return "null";
                }
            }
        }

        public long Id { get; }


        // Internal
        private static long GlobalId = -1;
        private static long GlobalInteractionId = -1;

        private readonly object _strongOwner;
        private readonly WeakReference<object> _weakOwner;
        private readonly string _ownerDescription;

        private Stack<string> _scopeStack = new();

        private readonly LogData[] _logBuffer;
        private readonly int _bufferSize;
        private long _currentLogIndex = -1;


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

            _bufferSize = Infrastructure.MaxLogCount;
            _logBuffer = new LogData[_bufferSize];
        }

        public static void ForceResetStaticProperties()
        {
            GlobalId = -1;
            GlobalInteractionId = -1;
        }

        #region Write and Exert
        public string Write(string message, string methodName)
        {
            if (string.IsNullOrWhiteSpace(message)) return message;
            if (Infrastructure.IsPrinted) return message;

            EnqueueLog(message, methodName, ScopeType.None);
            return message;
        }

        public DisposableHandle WriteScope(string message, string methodName)
        {
            var scopeMesage = message;
            if (TryMergeScope(out var exertedBy, out var prevMessage, out var prevInteractionId))
            {
                if (!string.IsNullOrEmpty(prevMessage))
                    scopeMesage = $"{message} <- {prevMessage}";
            }

            WriteInternal(scopeMesage, methodName, ScopeType.Open, exertedBy, prevInteractionId);

            _scopeStack.Push(methodName);
            return new DisposableHandle(this, message);
        }

        private string WriteInternal(string message, string methodName, ScopeType scopeType, Blackbox exertedBy = null, long interactionId = -1)
        {
            if (string.IsNullOrWhiteSpace(message)) return message;
            if (Infrastructure.IsPrinted) return message;

            EnqueueLog(message, methodName, scopeType, exertedBy, interactionId: interactionId);
            return message;
        }


        public string Exert(Blackbox other, string message, string methodName) =>
            ExertInternal(other, message, methodName, ScopeType.None);

        public DisposableHandle ExertScope(Blackbox other, string message, string methodName)
        {
            var scopeMesage = message;
            if (TryMergeScope(out var exertedBy, out var prevMessage, out var prevInteractionId))
            {
                if (!string.IsNullOrEmpty(prevMessage))
                    scopeMesage = $"{message} <- {prevMessage}";
            }

            ExertInternal(other, scopeMesage, methodName, ScopeType.Open, exertedBy, prevInteractionId);

            _scopeStack.Push(methodName);
            return new DisposableHandle(this, message);
        }

        private string ExertInternal(Blackbox exertingTo, string message, string methodName, ScopeType scopeType, Blackbox exertedBy = null, long interactionId = -1)
        {
            if (exertingTo == null)
                throw new ArgumentNullException(nameof(exertingTo));

            if (string.IsNullOrWhiteSpace(message)) return message;
            if (Infrastructure.IsPrinted) return message;

            if (exertingTo != this)
            {
                var currentInteractionId = interactionId >= 0 ? interactionId : Interlocked.Increment(ref GlobalInteractionId);

                exertingTo.EnqueueLog(message, methodName, ScopeType.None, this, null, currentInteractionId);
                EnqueueLog(message, methodName, scopeType, exertedBy, exertingTo, currentInteractionId);
            }
            else
                EnqueueLog(message, methodName, scopeType, exertedBy ?? this, this);

            return message;
        }

        public DisposableHandle ExertedScope(Blackbox exertedBy, string message, string methodName)
        {
            long interactionId = -1;
            string finalMessage = message;

            if (TryMergeScope(exertedBy, out var prevMessage, out var prevInteractionId))
            {
                interactionId = prevInteractionId;

                if (!string.IsNullOrEmpty(prevMessage))
                    finalMessage = $"{message} <- {prevMessage}";
            }

            if (interactionId == -1)
            {
                interactionId = Interlocked.Increment(ref GlobalInteractionId);

                if (exertedBy != null && exertedBy != this)
                {
                    exertedBy.EnqueueLog(message, methodName, ScopeType.None, null, this, interactionId);
                }
            }

            EnqueueLog(finalMessage, methodName, ScopeType.Open, exertedBy, null, interactionId);

            _scopeStack.Push(methodName);
            return new DisposableHandle(this, message);


            bool TryMergeScope(Blackbox expectedExertedBy, out string prevMessage, out long prevInteractionId)
            {
                var currentIndex = Interlocked.Read(ref _currentLogIndex);

                var prevIndex = (currentIndex + _bufferSize) % _bufferSize;
                var prev = _logBuffer[prevIndex];

                if (prev.Time != default
                    && prev.ScopeType == ScopeType.None
                    && prev.ExertedBy != null
                    && prev.ExertedBy != this
                    && (expectedExertedBy == null || prev.ExertedBy == expectedExertedBy))
                {
                    prevMessage = prev.Message;
                    prevInteractionId = prev.InteractionId;

                    Interlocked.Decrement(ref _currentLogIndex);
                    return true;
                }
                else
                {
                    prevMessage = default;
                    prevInteractionId = -1;
                    return false;
                }
            }
        }

        private bool TryMergeScope(out Blackbox exertedBy, out string prevMessage, out long prevInteractionId)
        {
            var currentIndex = Interlocked.Read(ref _currentLogIndex);

            var prevIndex = (currentIndex + _bufferSize) % _bufferSize;
            var prev = _logBuffer[prevIndex];

            if (prev.Time != default
                && prev.ScopeType == ScopeType.None // Avoid duplicate merge
                && prev.ExertedBy != null
                && prev.ExertedBy != this)
            {
                exertedBy = prev.ExertedBy;
                prevMessage = prev.Message;
                prevInteractionId = prev.InteractionId;

                Interlocked.Decrement(ref _currentLogIndex);
                return true;
            }
            else
            {
                exertedBy = null;
                prevMessage = default;
                prevInteractionId = -1;
                return false;
            }
        }

        internal void CloseScope(string scopeMessage)
        {
            if (_scopeStack.Count == 0)
                return;

            var scope = _scopeStack.Pop();
            EnqueueLog(scopeMessage, scope, ScopeType.Close);
        }
        #endregion


        private void EnqueueLog(string message, string methodName, ScopeType scopeType, Blackbox exertedBy = null, Blackbox exertingTo = null, long interactionId = -1) =>
            EnqueueLog(new LogData(this, _scopeStack.Count, message, methodName, scopeType, exertedBy, exertingTo, interactionId));
        private void EnqueueLog(LogData logData)
        {
            if (Infrastructure.IsPrinted)
                return;

            var currentIndex = Interlocked.Increment(ref _currentLogIndex);
            var bufferIndex = currentIndex % _bufferSize;

            _logBuffer[bufferIndex] = logData;
        }


        public IEnumerable<LogData> GetLogs()
        {
            var capturedIndex = Interlocked.Read(ref _currentLogIndex);
            var count = Math.Min(capturedIndex + 1, _bufferSize);
            var start = Math.Max(0, capturedIndex - _bufferSize + 1);

            for (var i = 0; i < count; i++)
            {
                var targetIndex = start + i;
                int bufferIndex = (int)(targetIndex % _bufferSize);
                
                var log = _logBuffer[bufferIndex];
                if (log.Time == default) continue; 
                
                yield return log;
            }
        }
    }
}
