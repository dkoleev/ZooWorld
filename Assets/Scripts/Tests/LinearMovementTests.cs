using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core.Movement;
using ZooWorld.Tests.Fakes;
using ZooWorld.Tests.Utils;

namespace ZooWorld.Tests
{
    public sealed class LinearMovementTests
    {
        private static LinearMovement Movement() => new(speed: 3f, acceleration: 10f);

        [Test]
        public void SpeedsUpNoFasterThanItsAcceleration()
        {
            var body = new FakeBody();
            Movement().Tick(body, Vector3.forward, 0.1f);

            CommonTestUtils.AreEqual(new Vector3(0f, 0f, 1f), body.Velocity);
        }
        
        [Test]
        public void SettlesAtTheConfiguredSpeed()
        {
            var body = new FakeBody();
            var movement = Movement();

            for (var i = 0; i < 100; i++)
                movement.Tick(body, Vector3.forward, 0.02f);

            CommonTestUtils.AreEqual(new Vector3(0f, 0f, 3f), body.Velocity);
        }
        
        [Test]
        public void LeavesVerticalVelocityToPhysics()
        {
            var body = new FakeBody { Velocity = new Vector3(0f, -5f, 0f) };

            Movement().Tick(body, Vector3.forward, 0.1f);

            Assert.That(body.Velocity.y, Is.EqualTo(-5f));
        }
        
        [Test]
        public void BrakesWhenThereIsNoDirection()
        {
            var body = new FakeBody { Velocity = new Vector3(2f, 0f, 0f) };

            Movement().Tick(body, Vector3.zero, 0.1f);

            CommonTestUtils.AreEqual(new Vector3(1f, 0f, 0f), body.Velocity);
        }

        [Test]
        public void RecoversFromAKnockBackGradually()
        {
            var body = new FakeBody { Velocity = new Vector3(0f, 0f, -6f) };

            Movement().Tick(body, Vector3.forward, 0.1f);

            // A collision threw it backward; one tick only wins back acceleration * dt.
            CommonTestUtils.AreEqual(new Vector3(0f, 0f, -5f), body.Velocity);
        }

        [Test]
        public void ZeroDeltaTimeChangesNothing()
        {
            var body = new FakeBody();

            Movement().Tick(body, Vector3.forward, 0f);

            CommonTestUtils.AreEqual(Vector3.zero, body.Velocity);
        }
    }
}