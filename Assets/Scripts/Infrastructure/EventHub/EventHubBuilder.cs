using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public interface IEventHubConnectionBuilder<TEventArgs>
    {
        public IEventHubConnectionBuilder<TEventArgs> OnUpdate(Action<TEventArgs> updated);
        public IEventHubConnectionBuilder<TEventArgs> OnUpdate(Func<TEventArgs, bool> selector, Action<TEventArgs> updated);

        public IEventHubConnectionBuilder<TEventArgs> OnDisposing(Action disposing);
        public IEventHubConnectionBuilder<TEventArgs> OnDisposed(Action disposed);
    }

    public partial class EventHub<TOwner, TEventArgs>
    {
        /// <summary>
        /// EventHubBuilder is for one-time use with a specific listener and must not be reused.
        /// </summary>
        private class Builder : IEventHubConnectionBuilder<TEventArgs>
        {
            // Property
            Connection _connection;


            // Content
            /// <summary>
            /// Do not call this method outside the EventHub class.
            /// </summary>
            internal Builder(Connection order) => _connection = order;

            public IEventHubConnectionBuilder<TEventArgs> OnUpdate(Action<TEventArgs> updated)
            {
                _connection.Updated += updated;
                return this;
            }
            public IEventHubConnectionBuilder<TEventArgs> OnUpdate(TEventArgs when, Action<TEventArgs> updated)
            {
                _connection.Updated += args =>
                {
                    if (EqualityComparer<TEventArgs>.Default.Equals(args, when))
                        updated?.Invoke(args);
                };
                return this;
            }
            public IEventHubConnectionBuilder<TEventArgs> OnUpdate(Func<TEventArgs, bool> selector, Action<TEventArgs> updated)
            {
                _connection.Updated += args =>
                {
                    if (selector(args))
                        updated?.Invoke(args);
                };
                return this;
            }

            public IEventHubConnectionBuilder<TEventArgs> OnDisposing(Action disposing)
            {
                _connection.Disposing += disposing;
                return this;
            }
            public IEventHubConnectionBuilder<TEventArgs> OnDisposed(Action disposed)
            {
                _connection.Disposed += disposed;
                return this;
            }
        }
    }
}
