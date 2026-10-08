using NUnit.Framework;
using ZooWorld.Core;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Events;
using ZooWorld.Core.Movement;
using ZooWorld.Tests.Fakes;

namespace ZooWorld.Tests
{
    public sealed class AnimalWorldTests
    {
        private FakeBroker<AnimalDiedEvent> _died;
        private FakeBroker<AnimalAteEvent> _ate;
        private AnimalWorld _world;

        [SetUp]
        public void SetUp()
        {
            _died = new FakeBroker<AnimalDiedEvent>();
            _ate = new FakeBroker<AnimalAteEvent>();
            _world = new AnimalWorld(_died, _ate);
        }

        private Animal Add(int id, IDiet diet, IMovement movement = null, string species = "animal")
        {
            var animal = Utils.CommonTestUtils.Create(id, diet, movement, species);
            _world.Add(animal);
            return animal;
        }

        [Test]
        public void AnimalsListsOnlyTheLivingWithTheirSpecies()
        {
            var frog = Add(0, Diets.Prey, species: "Frog");
            var snake = Add(1, Diets.Predator, species: "Snake");

            _world.Collide(frog, snake);

            Assert.That(_world.Animals, Is.EqualTo(new[] { snake }));
            Assert.That(_world.Animals[0].Species, Is.EqualTo("Snake"));
        }

        [Test]
        public void TicksEveryAnimalItHolds()
        {
            var first = new FakeMovement();
            var second = new FakeMovement();
            Add(0, Diets.Prey, first);
            Add(1, Diets.Predator, second);

            _world.Tick(0.02f);

            Assert.That(first.Ticks, Is.EqualTo(1));
            Assert.That(second.Ticks, Is.EqualTo(1));
        }

        [Test]
        public void PredatorMeetingPrey_KillsThePreyAndAnnouncesBoth()
        {
            var frog = Add(0, Diets.Prey);
            var snake = Add(1, Diets.Predator);

            _world.Collide(frog, snake);

            Assert.That(frog.IsAlive, Is.False);
            Assert.That(snake.IsAlive, Is.True);
            Assert.That(_world.AliveCount, Is.EqualTo(1));
            Assert.That(_died.Published, Has.Count.EqualTo(1));
            Assert.That(_died.Published[0].animal, Is.SameAs(frog));
            Assert.That(_ate.Published, Has.Count.EqualTo(1));
            Assert.That(_ate.Published[0].ate, Is.SameAs(snake));
        }

        [Test]
        public void TheSecondCallbackForTheSameCollisionChangesNothing()
        {
            var frog = Add(0, Diets.Prey);
            var snake = Add(1, Diets.Predator);

            // Unity reports one contact to both bodies.
            _world.Collide(frog, snake);
            _world.Collide(snake, frog);

            Assert.That(_died.Published, Has.Count.EqualTo(1));
            Assert.That(_ate.Published, Has.Count.EqualTo(1));
        }

        [Test]
        public void ADeadAnimalNeitherEatsNorDiesAgain()
        {
            var older = Add(0, Diets.Predator);
            var newer = Add(1, Diets.Predator);
            var frog = Add(2, Diets.Prey);
            _world.Collide(older, newer);

            // Contacts from the same physics step can still name the snake that just died.
            _world.Collide(newer, frog);
            _world.Collide(older, newer);

            Assert.That(frog.IsAlive, Is.True);
            Assert.That(_died.Published, Has.Count.EqualTo(1));
            Assert.That(_world.AliveCount, Is.EqualTo(2));
        }

        [Test]
        public void TwoPreyBumpingIntoEachOtherBothLive()
        {
            var frog = Add(0, Diets.Prey);
            var otherFrog = Add(1, Diets.Prey);

            _world.Collide(frog, otherFrog);

            Assert.That(_world.AliveCount, Is.EqualTo(2));
            Assert.That(_died.Published, Is.Empty);
            Assert.That(_ate.Published, Is.Empty);
        }

        [Test]
        public void DeadAnimalsAreNoLongerTicked()
        {
            var movement = new FakeMovement();
            var frog = Add(0, Diets.Prey, movement);
            var snake = Add(1, Diets.Predator);
            _world.Collide(frog, snake);

            _world.Tick(0.02f);

            Assert.That(movement.Ticks, Is.EqualTo(0));
        }

        [Test]
        public void TheEatenAnimalLeavesTheFieldAtOnce()
        {
            var frog = Add(0, Diets.Prey);
            var snake = Add(1, Diets.Predator);

            _world.Collide(frog, snake);

            Assert.That(((FakeBody)frog.Body).Despawned, Is.True);
            Assert.That(((FakeBody)snake.Body).Despawned, Is.False);
        }

        [Test]
        public void AListenerThatThrowsDoesNotLeaveTheWorldHalfUpdated()
        {
            var frog = Add(0, Diets.Prey);
            var snake = Add(1, Diets.Predator);
            _died.Subscribe(new ThrowingHandler());

            Assert.Throws<System.InvalidOperationException>(() => _world.Collide(frog, snake));

            // The death is complete and the meal is still announced.
            Assert.That(((FakeBody)frog.Body).Despawned, Is.True);
            Assert.That(_world.AliveCount, Is.EqualTo(1));
            Assert.That(_ate.Published, Has.Count.EqualTo(1));
        }

        private sealed class ThrowingHandler : MessagePipe.IMessageHandler<AnimalDiedEvent>
        {
            public void Handle(AnimalDiedEvent message) => throw new System.InvalidOperationException("listener failed");
        }
    }
}
