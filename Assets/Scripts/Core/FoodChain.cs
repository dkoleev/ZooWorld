using ZooWorld.Core.Animals;

namespace ZooWorld.Core
{
    public static class FoodChain
    {
        public static Animal Resolve(Animal a, Animal b)
        {
            var aEatsB = a.Diet.CanEat(b.Diet);
            var bEatsA = b.Diet.CanEat(a.Diet);

            return aEatsB switch
            {
                // Each would eat the other: the one that has lived longer wins.
                true when bEatsA => a.Id > b.Id ? a : b,
                true => b,
                _ => bEatsA ? a : null
            };
        }
    }
}
