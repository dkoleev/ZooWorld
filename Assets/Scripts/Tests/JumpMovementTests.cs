using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core.Movement;
using ZooWorld.Tests.Fakes;
using ZooWorld.Tests.Utils;

namespace ZooWorld.Tests
{
    public sealed class JumpMovementTests
    {
        private const float Gravity = 9.81f;

        private static JumpMovement Movement(float distance = 3f) => new(interval: 2f, distance: distance, gravity: Gravity);

        [Test]
        public void WaitsForTheInterval()
        {
            var body = new FakeBody();

            Movement().Tick(body, Vector3.forward, 1.5f);

            Assert.That(body.AddedVelocityBuffer, Is.Empty);
        }

        [Test]
        public void JumpsExactlyTheConfiguredDistance()
        {
            var body = new FakeBody();
            var distance = 3f;

            Movement(distance).Tick(body, Vector3.forward, 2f);

            var launch = body.Velocity;
            var flightTime = 2f * launch.y / Gravity;
            Assert.That(launch.z * flightTime, Is.EqualTo(distance).Within(1e-3f));
            Assert.That(launch.x, Is.EqualTo(0f));
        }

        [Test]
        public void StaysPutWhileAirborneAndJumpsOnLanding()
        {
            var body = new FakeBody { IsGrounded = false };
            var movement = Movement();

            movement.Tick(body, Vector3.forward, 2.5f);
            Assert.That(body.AddedVelocityBuffer, Is.Empty);

            body.IsGrounded = true;
            movement.Tick(body, Vector3.forward, 0.02f);
            Assert.That(body.AddedVelocityBuffer, Has.Count.EqualTo(1));
        }

        [Test]
        public void WaitsAFullIntervalBetweenJumps()
        {
            var body = new FakeBody();
            var movement = Movement();

            movement.Tick(body, Vector3.forward, 2f);
            movement.Tick(body, Vector3.forward, 1.5f);
            Assert.That(body.AddedVelocityBuffer, Has.Count.EqualTo(1));

            movement.Tick(body, Vector3.forward, 0.5f);
            Assert.That(body.AddedVelocityBuffer, Has.Count.EqualTo(2));
        }

        [Test]
        public void NegativeDistanceNeverProducesNaN()
        {
            var body = new FakeBody();

            Movement(distance: -4f).Tick(body, Vector3.forward, 2f);

            Assert.That(float.IsNaN(body.Velocity.x + body.Velocity.y + body.Velocity.z), Is.False);
        }

        [Test]
        public void JumpsTheSameWayWhateverItWasDoingBefore()
        {
            var resting = new FakeBody();
            var shoved = new FakeBody { Velocity = new Vector3(3f, 0f, -2f) };

            Movement().Tick(resting, Vector3.forward, 2f);
            Movement().Tick(shoved, Vector3.forward, 2f);

            // A frog still sliding from a collision must not carry that speed into its hop.
            CommonTestUtils.AreEqual(resting.Velocity, shoved.Velocity);
        }
    }
}
