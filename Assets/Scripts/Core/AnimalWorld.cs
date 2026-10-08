using System.Collections.Generic;
using MessagePipe;
using VContainer;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Events;

namespace ZooWorld.Core
{
    public class AnimalWorld
    {
        private readonly List<Animal> _animals = new();
        private readonly IPublisher<AnimalDiedEvent> _died;
        private readonly IPublisher<AnimalAteEvent> _ate;

        [Inject]
        public AnimalWorld(IPublisher<AnimalDiedEvent> died, IPublisher<AnimalAteEvent> ate)
        {
            _died = died;
            _ate = ate;
        }

        public int AliveCount => _animals.Count;
        public IReadOnlyList<Animal> Animals => _animals;

        public void Add(Animal animal) => _animals.Add(animal);

        public void Tick(float deltaTime)
        {
            for (var i = 0; i < _animals.Count; i++)
                _animals[i].Tick(deltaTime);
        }

        public void Collide(Animal a, Animal b)
        {
            if (!a.IsAlive || !b.IsAlive)
                return;

            var eaten = FoodChain.Resolve(a, b);
            if (eaten == null)
                return;

            var predator = eaten == a ? b : a;
            eaten.Kill();
            _animals.Remove(eaten);
            
            _ate.Publish(new AnimalAteEvent(predator, eaten));
            _died.Publish(new AnimalDiedEvent(eaten));
        }
    }
}
