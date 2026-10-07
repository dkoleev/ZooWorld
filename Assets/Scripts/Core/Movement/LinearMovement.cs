using UnityEngine;

namespace ZooWorld.Core.Movement
{
    public class LinearMovement : IMovement
    {
        private readonly float _speed;
        private readonly float _acceleration;

        public LinearMovement(float speed, float acceleration)
        {
            _speed = speed;
            _acceleration = acceleration;
        }

        public void Tick(IEntityBody body, Vector3 direction, float deltaTime)
        {
            var horizontal = body.Velocity;
            horizontal.y = 0f;
            var correction = direction * _speed - horizontal;
            body.AddVelocity(Vector3.ClampMagnitude(correction, _acceleration * deltaTime));
        }
    }
}
