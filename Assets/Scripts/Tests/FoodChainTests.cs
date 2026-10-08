using NUnit.Framework;
using ZooWorld.Core;
using ZooWorld.Tests.Fakes;

namespace ZooWorld.Tests
{
    public sealed class FoodChainTests
    {
        [Test]
        public void TwoPrey_NobodyIsEaten()
        {
            var frog = Utils.CommonTestUtils.Create(0, Diets.Prey);
            var otherFrog = Utils.CommonTestUtils.Create(1, Diets.Prey);

            Assert.That(FoodChain.Resolve(frog, otherFrog), Is.Null);
        }

        [Test]
        public void PredatorEatsPrey_WhicheverSideItIsOn()
        {
            var frog = Utils.CommonTestUtils.Create(0, Diets.Prey);
            var snake = Utils.CommonTestUtils.Create(1, Diets.Predator);

            Assert.That(FoodChain.Resolve(snake, frog), Is.SameAs(frog));
            Assert.That(FoodChain.Resolve(frog, snake), Is.SameAs(frog));
        }

        [Test]
        public void TwoPredators_TheOneSpawnedLaterIsEaten()
        {
            var older = Utils.CommonTestUtils.Create(3, Diets.Predator);
            var newer = Utils.CommonTestUtils.Create(7, Diets.Predator);

            Assert.That(FoodChain.Resolve(older, newer), Is.SameAs(newer));
            Assert.That(FoodChain.Resolve(newer, older), Is.SameAs(newer));
        }

        [Test]
        public void ADietDecidesForItselfWhatItEats()
        {
            // A hawk hunts snakes only: frogs are safe from it, and snakes cannot eat it back.
            var hawkDiet = new FakeDiet("hawk");
            hawkDiet.Eats(Diets.Predator);
            var hawk = Utils.CommonTestUtils.Create(9, hawkDiet);
            var snake = Utils.CommonTestUtils.Create(1, Diets.Predator);
            var frog = Utils.CommonTestUtils.Create(2, Diets.Prey);

            Assert.That(FoodChain.Resolve(hawk, frog), Is.Null);
            Assert.That(FoodChain.Resolve(snake, hawk), Is.SameAs(snake));
            Assert.That(FoodChain.Resolve(hawk, snake), Is.SameAs(snake));
        }
    }
}
