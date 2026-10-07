using System;
using System.Collections.Generic;
using MessagePipe;
using ZooWorld.Core.Events;

namespace ZooWorld.Core
{
    public class DeathStats : IMessageHandler<AnimalDiedEvent>, IDisposable
    {
        public event Action Changed;
        public int GetDeaths(string dietId) => _deaths.GetValueOrDefault(dietId, 0);
        
        private readonly Dictionary<string, int> _deaths = new();
        private readonly IDisposable _subscription;

        public DeathStats(ISubscriber<AnimalDiedEvent> died) => _subscription = died.Subscribe(this);

        public void Handle(AnimalDiedEvent message)
        {
            var dietId = message.animal.Diet.Id;
            _deaths[dietId] = GetDeaths(dietId) + 1;
            Changed?.Invoke();
        }

        public void Dispose() => _subscription.Dispose();
    }
}
