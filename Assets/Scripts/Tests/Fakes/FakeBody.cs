using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Core;

namespace ZooWorld.Tests.Fakes
{
    internal sealed class FakeBody : IEntityBody
    {
        public Vector3 Position { get; }
        public Vector3 DisplayPosition { get; }
        public Vector3 Velocity { get; set; }
        public bool IsGrounded { get; set; } = true;
        public Vector3 Facing { get; private set; }
        public bool Despawned { get; private set; }

        public readonly List<Vector3> AddedVelocityBuffer = new();

        public void AddVelocity(Vector3 delta)
        {
            AddedVelocityBuffer.Add(delta);
            Velocity += delta;
        }

        public void Face(Vector3 direction)
        {
            Facing = direction;
        }

        public void Despawn()
        {
            Despawned = true;
        }
    }
}
