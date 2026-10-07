using ZooWorld.Core.Animals;

namespace ZooWorld.Core.Events
{
    public readonly struct AnimalDiedEvent
    {
        public readonly Animal animal;
        
        public AnimalDiedEvent(Animal animal)
        {
            this.animal = animal;
        }
    }
}
