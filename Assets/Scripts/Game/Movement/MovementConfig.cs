using UnityEngine;
using ZooWorld.Core.Movement;

namespace ZooWorld.Game.Game.Movement
{
    public abstract class MovementConfig : ScriptableObject
    {
        public abstract IMovement Create();
    }
}