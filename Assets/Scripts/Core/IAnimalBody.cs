using UnityEngine;

namespace ZooWorld.Core
{
    public interface IAnimalBody
    {
        public Vector3 Position { get; }
        public Vector3 DisplayPosition { get; }
        public Vector3 Velocity { get; }
        public bool IsGrounded { get; }

        /// <summary>Changes velocity at once, regardless of mass.</summary>
        void AddVelocity(Vector3 delta);

        /// <summary>Turns the visual toward a horizontal direction. Physics is not affected.</summary>
        void Face(Vector3 direction);

        /// <summary>Takes the body off the field once its animal is dead.</summary>
        void Despawn();
    }
}
