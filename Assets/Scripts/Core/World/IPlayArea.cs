using UnityEngine;

namespace ZooWorld.Core.World
{
    public interface IPlayArea
    {
        Vector3 Center { get; }
        bool Contains(Vector3 position);
        Vector3 GetRandomPoint(IRandom random);
    }
}
