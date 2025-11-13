using System;
using System.Collections.Generic;
using Infrastructure;
using UnityEngine;

namespace Infrastructure
{
    public interface IEventSubject<TOwner, TEventArgs> where TOwner : class
    {
        EventHub<TOwner, TEventArgs> EventHub { get; }
    }

    public partial class EventHub<TOwner, TEventArgs> : IDisposable where TOwner : class
    {
        // Front
        public TOwner Owner { get; }
        public int ConnectionCount => _connections?.Count ?? 0;
        public bool IsDisposed { get; private set; } = false;


        // Internal
        private class Connection
        {
            public Action<TEventArgs> Updated;
            public Action Disposing;
            public Action Disposed;
        }

        private AdaptiveDictionary<object, Connection> _connections = new();

        private bool _isUpdating = false;
        private bool _isDisposing = false;


        // Content
        public EventHub(TOwner owner) => Owner = owner;

        public IEventHubConnectionBuilder<TEventArgs> Connect(object listener)
        {
            ThrowIfDisposed();

            if (listener == null)
                throw new ArgumentNullException(nameof(listener), "[EventHub] You cannot register a null listener.");
            if (_connections.ContainsKey(listener))
                throw new InvalidOperationException($"[EventHub] This Listener already exists: {listener}");

            var order = new Connection();
            _connections.Add(listener, order);

            return new Builder(order);
        }

        public bool HasListener(object listener) => _connections?.ContainsKey(listener) ?? false;

        public void Disconnect(object listener)
        {
            ThrowIfDisposed();

            if (listener == null) return;
            _connections.Remove(listener);
        }

        /// <summary>
        /// This method can only be invoked by the Owner
        /// </summary>
        public virtual void Update(TEventArgs args)
        {
            ThrowIfDisposed();

            if (_isUpdating)
            {
                Debug.LogWarning("[EventHub] Update ignored because EventHub is already being updated.");
                return;
            }
            _isUpdating = true;

            Queue<object> toDisconnect = null;

            foreach (var (listener, order) in _connections)
            {
                try
                {
                    order.Updated?.Invoke(args);
                }
                catch (Exception ex)
                {
                    toDisconnect ??= new();
                    toDisconnect.Enqueue(listener);

                    Debug.LogException(ex);
                }
            }

            if (toDisconnect != null)
            {
                while (toDisconnect.Count > 0)
                    Disconnect(toDisconnect.Dequeue());
            }

            _isUpdating = false;
        }


        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(EventHub<TOwner, TEventArgs>));
        }


        private void Dispose(bool disposing)
        {
            if (_isDisposing) return;
            _isDisposing = true;

            if (!disposing && !IsDisposed)
            {
                if (Application.isPlaying) Debug.LogWarning(
                    "The EventHub object was garbage collected without being disposed.\n" +
                    "Ensure that Dispose() is explicitly called when the object is no longer needed.\n" +
                    $"Owner :\n{Owner}");
            }

            if (disposing)
            {
                foreach (var order in _connections.Values)
                {
                    try
                    {
                        order.Disposing?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                IsDisposed = true;

                foreach (var order in _connections.Values)
                {
                    try
                    {
                        order.Disposed?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                GC.SuppressFinalize(this);
            }

            _connections = null;
        }
        /// <summary>
        /// This method can only be invoked by the Owner.
        /// </summary>
        public void Dispose() => Dispose(true);
        ~EventHub() => Dispose(false);
    }
}
