using UnityEngine;
using ZooWorld.Core.Movement;

namespace ZooWorld.Game.Movement
{
    public abstract class MovementConfig : ScriptableObject
    {
        public abstract IMovement Create();
    }
}