using UnityEngine;
using ZooWorld.Core;
using ZooWorld.Core.Movement;

namespace ZooWorld.Tests.Fakes
{
    internal sealed class FakeMovement : IMovement
    {
        public int Ticks { get; private set; }
        public Vector3 LastDirection { get; private set; }

        public void Tick(IEntityBody target, Vector3 direction, float deltaTime)
        {
            Ticks++;
            LastDirection = direction;
        }
    }
}