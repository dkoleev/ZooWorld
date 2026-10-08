using UnityEngine;
using ZooWorld.Core;
using ZooWorld.Core.Movement;

namespace ZooWorld.Tests.Fakes
{
    internal sealed class FakeMovement : IMovement
    {
        public int Ticks { get; private set; }
        public Vector3 LastDirection { get; private set; }

        public void Tick(IAnimalBody target, Vector3 direction, float deltaTime)
        {
            Ticks++;
            LastDirection = direction;
        }
    }
}