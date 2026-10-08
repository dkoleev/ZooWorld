using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core;
using ZooWorld.Tests.Fakes;
using ZooWorld.Tests.Utils;

namespace ZooWorld.Tests
{
    public sealed class WanderDirectionTests
    {
        // FakeRandom.Fraction 0 picks angle 0; 0.25 picks a quarter turn.
        private static readonly Vector3 East = new(1f, 0f, 0f);
        private static readonly Vector3 North = new(0f, 0f, 1f);

        private FakePlayArea _area;
        private FakeRandom _random;

        [SetUp]
        public void SetUp()
        {
            _area = new FakePlayArea { HalfSize = 10f };
            _random = new FakeRandom { Fraction = 0f };
        }

        [Test]
        public void KeepsItsDirectionUntilTheIntervalElapses()
        {
            var wander = new WanderDirection(_area, _random, 2f);
            _random.Fraction = 0.25f;

            CommonTestUtils.AreEqual(East, wander.Tick(Vector3.zero, 1f));
            CommonTestUtils.AreEqual(East, wander.Tick(Vector3.zero, 0.5f));
        }

        [Test]
        public void PicksANewDirectionWhenTheIntervalElapses()
        {
            var wander = new WanderDirection(_area, _random, 2f);
            _random.Fraction = 0.25f;
            wander.Tick(Vector3.zero, 1f);

            CommonTestUtils.AreEqual(North, wander.Tick(Vector3.zero, 1f));
        }

        [Test]
        public void PointsAtTheCentreWhenOutsideTheArea()
        {
            var wander = new WanderDirection(_area, _random, 2f);

            // Height is ignored: the direction stays horizontal.
            CommonTestUtils.AreEqual(Vector3.left, wander.Tick(new Vector3(20f, 3f, 0f), 0.02f));
        }

        [Test]
        public void KeepsHeadingInwardForAFullIntervalAfterComingBack()
        {
            var wander = new WanderDirection(_area, _random, 2f);
            _random.Fraction = 0.25f;
            wander.Tick(new Vector3(20f, 0f, 0f), 1.5f);

            CommonTestUtils.AreEqual(Vector3.left, wander.Tick(new Vector3(9f, 0f, 0f), 1.5f));
        }

        [Test]
        public void TurnsBackWhenTheAreaShrinksAroundIt()
        {
            var wander = new WanderDirection(_area, _random, 2f);
            var position = new Vector3(5f, 0f, 0f);
            CommonTestUtils.AreEqual(East, wander.Tick(position, 0.02f));

            _area.HalfSize = 2f;

            CommonTestUtils.AreEqual(Vector3.left, wander.Tick(position, 0.02f));
        }

        [Test]
        public void ZeroIntervalPicksANewDirectionEveryTick()
        {
            var wander = new WanderDirection(_area, _random, 0f);

            _random.Fraction = 0.25f;
            CommonTestUtils.AreEqual(North, wander.Tick(Vector3.zero, 0.02f));
            _random.Fraction = 0f;
            CommonTestUtils.AreEqual(East, wander.Tick(Vector3.zero, 0.02f));
        }
    }
}