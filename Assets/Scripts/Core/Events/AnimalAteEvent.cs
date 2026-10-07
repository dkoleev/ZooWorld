using ZooWorld.Core.Animals;

namespace ZooWorld.Core.Events
{
    public readonly struct AnimalAteEvent
    {
        public readonly Animal ate;
        public readonly Animal eaten;

        public AnimalAteEvent(Animal ate, Animal eaten)
        {
            this.ate = ate;
            this.eaten = eaten;
        }
    }
}
