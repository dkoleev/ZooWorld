using UnityEngine;

namespace ZooWorld.Core.Movement
{
    public interface IMovement
    {
        void Tick(IEntityBody target, Vector3 direction, float deltaTime);
    }
}