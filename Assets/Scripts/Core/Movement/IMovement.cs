using UnityEngine;

namespace ZooWorld.Core.Movement
{
    public interface IMovement
    {
        void Tick(IAnimalBody target, Vector3 direction, float deltaTime);
    }
}