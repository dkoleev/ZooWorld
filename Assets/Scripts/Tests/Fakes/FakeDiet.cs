using System.Collections.Generic;
using ZooWorld.Core.Animals;

namespace ZooWorld.Tests.Fakes
{
    internal sealed class FakeDiet : IDiet
    {
        private readonly HashSet<IDiet> _eats = new();

        public FakeDiet(string id) => Id = id;

        public string Id { get; }

        public bool CanEat(IDiet other) => _eats.Contains(other);

        public void Eats(params IDiet[] diets) => _eats.UnionWith(diets);
    }
    
    internal static class Diets
    {
        public static readonly IDiet Prey = new FakeDiet("prey");
        public static readonly IDiet Predator = CreatePredator();

        private static IDiet CreatePredator()
        {
            var predator = new FakeDiet("predator");
            predator.Eats(Prey, predator);
            return predator;
        }
    }
}
