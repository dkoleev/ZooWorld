using NUnit.Framework;
using ZooWorld.Core;
using ZooWorld.Core.Events;
using ZooWorld.Tests.Fakes;

namespace ZooWorld.Tests
{
    public class DeathStatsTests
    {
        private FakeBroker<AnimalDiedEvent> _died;
        private DeathStats _stats;

        [SetUp]
        public void SetUp()
        {
            _died = new FakeBroker<AnimalDiedEvent>();
            _stats = new DeathStats(_died);
        }

        [Test]
        public void CountsEachDietSeparately()
        {
            _died.Publish(new AnimalDiedEvent(Utils.CommonTestUtils.Create(0, Diets.Prey)));
            _died.Publish(new AnimalDiedEvent(Utils.CommonTestUtils.Create(1, Diets.Prey)));
            _died.Publish(new AnimalDiedEvent(Utils.CommonTestUtils.Create(2, Diets.Predator)));

            Assert.That(_stats.GetDeaths("prey"), Is.EqualTo(2));
            Assert.That(_stats.GetDeaths("predator"), Is.EqualTo(1));
        }

        [Test]
        public void CountsADietItHasNeverHeardOf()
        {
            _died.Publish(new AnimalDiedEvent(Utils.CommonTestUtils.Create(0, new FakeDiet("scavenger"))));

            Assert.That(_stats.GetDeaths("scavenger"), Is.EqualTo(1));
            Assert.That(_stats.GetDeaths("prey"), Is.EqualTo(0));
        }

        [Test]
        public void RaisesChangedForEveryDeath()
        {
            var raised = 0;
            _stats.Changed += () => raised++;

            _died.Publish(new AnimalDiedEvent(Utils.CommonTestUtils.Create(0, Diets.Prey)));
            _died.Publish(new AnimalDiedEvent(Utils.CommonTestUtils.Create(1, Diets.Predator)));

            Assert.That(raised, Is.EqualTo(2));
        }

        [Test]
        public void StopsCountingAfterDispose()
        {
            _stats.Dispose();

            _died.Publish(new AnimalDiedEvent(Utils.CommonTestUtils.Create(0, Diets.Prey)));

            Assert.That(_stats.GetDeaths("prey"), Is.EqualTo(0));
        }
    }
}
