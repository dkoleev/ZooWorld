using UnityEngine;
using ZooWorld.Core;
using ZooWorld.Core.World;

namespace ZooWorld.Tests.Fakes
{
    internal sealed class FakePlayArea : IPlayArea
    {
        public Vector3 Center { get; set; }
        public float HalfSize { get; set; } = 10f;

        public bool Contains(Vector3 position) =>
            Mathf.Abs(position.x - Center.x) <= HalfSize && Mathf.Abs(position.z - Center.z) <= HalfSize;

        public Vector3 GetRandomPoint(IRandom random) => Center;
    }
}