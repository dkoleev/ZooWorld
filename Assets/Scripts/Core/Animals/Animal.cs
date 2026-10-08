using ZooWorld.Core.Movement;

namespace ZooWorld.Core.Animals
{
    public class Animal
    {
        /// <summary>Spawn sequence number: the lower it is, the longer the animal has lived.</summary>
        public int Id { get; }

        /// <summary>Which kind of animal this is, e.g. "Frog". Two species may share a diet.</summary>
        public string Species { get; }

        public IDiet Diet { get; }
        public bool IsAlive { get; private set; } = true;
        public IAnimalBody Body => _body;

        private readonly IAnimalBody _body;
        private readonly IMovement _movement;
        private readonly WanderDirection _wander;

        public Animal(int id, string species, IDiet diet, IAnimalBody body, IMovement movement, WanderDirection wander)
        {
            Id = id;
            Species = species;
            Diet = diet;
            _body = body;
            _movement = movement;
            _wander = wander;
        }

        internal void Kill()
        {
            IsAlive = false;
            _body.Despawn();
        }

        public void Tick(float deltaTime)
        {
            if (!IsAlive)
                return;

            var direction = _wander.Tick(_body.Position, deltaTime);
            _body.Face(direction);
            _movement.Tick(_body, direction, deltaTime);
        }
    }
}
