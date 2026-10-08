using UnityEngine;
using UnityEngine.Pool;
using ZooWorld.Core;
using ZooWorld.Core.Animals;

namespace ZooWorld.Game.Game.Animals
{
    [RequireComponent(typeof(Rigidbody))]
    public class AnimalView : MonoBehaviour, IEntityBody
    {
        private const float TurnSharpness = 0.2f;

        [SerializeField] private Rigidbody body;
        [Tooltip("Rotated to face the movement direction; the collider never turns.")]
        [SerializeField] private Transform visual;
        [Tooltip("Ray length from the body centre: collider radius plus a little slack.")]
        [SerializeField, Min(0f)] private float groundCheckDistance = 0.5f;

        private AnimalWorld _world;
        private IObjectPool<AnimalView> _pool;

        public Animal Animal { get; private set; }

        public bool IsWired => body != null && visual != null;

        public Vector3 Position => body.position;

        public Vector3 DisplayPosition => transform.position;
        public Vector3 Velocity => body.linearVelocity;

        public bool IsGrounded => Physics.Raycast(body.position, Vector3.down, groundCheckDistance);

        public void AddVelocity(Vector3 delta) => body.AddForce(delta, ForceMode.VelocityChange);

        public void Face(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0f)
                visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(direction), TurnSharpness);
        }

        public void Spawn(Animal animal, AnimalWorld world, IObjectPool<AnimalView> pool, Vector3 position)
        {
            Animal = animal;
            _world = world;
            _pool = pool;
            // Moved while inactive, so the rigidbody wakes up already in place.
            transform.SetPositionAndRotation(position, Quaternion.identity);
            gameObject.SetActive(true);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        public void Despawn() => _pool.Release(this);

        private void OnCollisionEnter(Collision collision)
        {
            // The ground is a static collider and has no rigidbody.
            if (collision.rigidbody != null && collision.rigidbody.TryGetComponent(out AnimalView other))
                _world.Collide(Animal, other.Animal);
        }
    }
}
