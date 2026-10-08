using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core;

namespace ZooWorld.Game.Animals
{
    public class AnimalSimulation : IFixedTickable
    {
        private readonly AnimalWorld _world;

        [Inject]
        public AnimalSimulation(AnimalWorld world)
        {
            _world = world;
        }

        public void FixedTick()
        {
            _world.Tick(Time.fixedDeltaTime);
        }
    }
}
