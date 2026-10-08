using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core;
using ZooWorld.Core.Animals;
using ZooWorld.Tests.Fakes;
using ZooWorld.Tests.Utils;

namespace ZooWorld.Tests
{
    public sealed class AnimalsTests
    {
        [Test]
        public void TickMovesAndFacesAlongItsWanderDirection()
        {
            var body = new FakeBody();
            var movement = new FakeMovement();
            var wander = new WanderDirection(new FakePlayArea(), new FakeRandom(), new Vector2(1f, 1f));
            var animal = new Animal(0, Diets.Prey, body, movement, wander);
            
            animal.Tick(0.02f);

            // FakeRandom's default picks angle 0, which is +X.
            CommonTestUtils.AreEqual(Vector3.right, movement.LastDirection);
            CommonTestUtils.AreEqual(Vector3.right, body.Facing);
        }
    }
}