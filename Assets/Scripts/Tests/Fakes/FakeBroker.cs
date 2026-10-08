using System;
using System.Collections.Generic;
using MessagePipe;

namespace ZooWorld.Tests.Fakes
{
    /// <summary>In-memory stand-in for a MessagePipe broker: records what was published and delivers it.</summary>
    public class FakeBroker<T> : IPublisher<T>, ISubscriber<T>
    {
        public readonly List<T> Published = new();
        private readonly List<IMessageHandler<T>> _handlers = new();

        public void Publish(T message)
        {
            Published.Add(message);
            foreach (var handler in _handlers.ToArray())
                handler.Handle(message);
        }

        public IDisposable Subscribe(IMessageHandler<T> handler, params MessageHandlerFilter<T>[] filters)
        {
            _handlers.Add(handler);
            return new Subscription(() => _handlers.Remove(handler));
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _dispose;
            public Subscription(Action dispose) => _dispose = dispose;
            public void Dispose() => _dispose();
        }
    }
}
