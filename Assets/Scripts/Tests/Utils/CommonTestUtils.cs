using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Movement;
using ZooWorld.Tests.Fakes;

namespace ZooWorld.Tests.Utils
{
    internal static class CommonTestUtils
    {
        public static void AreEqual(Vector3 expected, Vector3 actual) =>
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(1e-4f), $"Expected {expected:F4} but was {actual:F4}");
        
        public static Animal Create(int id, IDiet diet, IMovement movement = null, string species = "animal") =>
            new(id, species, diet, new FakeBody(), movement ?? new FakeMovement(), new WanderDirection(new FakePlayArea(), new FakeRandom(), new Vector2(1f, 1f)));
    }
}