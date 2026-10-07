using UnityEngine;
using VContainer.Unity;
using ZooWorld.Core;

namespace ZooWorld.Game.Game.Animals
{
    public class AnimalSimulation : IFixedTickable
    {
        private readonly AnimalWorld _world;

        public AnimalSimulation(AnimalWorld world) => _world = world;

        public void FixedTick() => _world.Tick(Time.fixedDeltaTime);
    }
}
