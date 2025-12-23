#if UNIENGINE
using System;

namespace UniEngine.StateMachines.FSM
{
    public partial class Work
    {
        private class StreamManager : IDisposable
        {
            // Internal
            private Work owner;

            private interface IStreamHandler : IDisposable
            {
                void Subscribe(Action action);
                void Unsubscribe();
            }
            private IStreamHandler streamHandler = null;


            // Content
            public StreamManager(Work owner) => this.owner = owner;

            public void SetStream(EventStream stream)
            {
                RemoveStream();
                streamHandler = new DirectStreamHandler(stream);
            }
            public void SetStream(Func<EventStream> streamFactory)
            {
                RemoveStream();
                streamHandler = new StreamFactoryHandler(streamFactory);
            }
            public void RemoveStream()
            {
                streamHandler?.Dispose();
                streamHandler = null;
            }

            public void Enter() => streamHandler?.Subscribe(owner.Update);
            public void Exit() => streamHandler?.Unsubscribe();

            public void Dispose() => RemoveStream();

            
            // Subclasses
            private class DirectStreamHandler : IStreamHandler
            {
                private EventStream stream;
                private IDisposable subscriptionHandle;

                public DirectStreamHandler(EventStream stream)
                {
                    this.stream = stream;
                }

                public void Subscribe(Action action)
                {
                    Unsubscribe();
                    subscriptionHandle = stream.Subscribe(action);
                }
                public void Unsubscribe()
                {
                    subscriptionHandle?.Dispose();
                    subscriptionHandle = null;
                }

                public void Dispose()
                {
                    Unsubscribe();
                }
            }

            private class StreamFactoryHandler : IStreamHandler
            {
                private Func<EventStream> streamFactory;
                private EventStream stream;
                private IDisposable subscriptionHandle;

                public StreamFactoryHandler(Func<EventStream> streamFactory)
                {
                    this.streamFactory = streamFactory;
                }

                public void Subscribe(Action action)
                {
                    Unsubscribe();

                    stream = streamFactory();
                    subscriptionHandle = stream.Subscribe(action);
                }
                public void Unsubscribe()
                {
                    subscriptionHandle?.Dispose();
                    subscriptionHandle = null;

                    stream?.Dispose();
                    stream = null;
                }

                public void Dispose()
                {
                    Unsubscribe();
                }
            }
        }
    }
}
#endif
