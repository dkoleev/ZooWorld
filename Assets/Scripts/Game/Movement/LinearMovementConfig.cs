using UnityEngine;
using ZooWorld.Core.Movement;

namespace ZooWorld.Game.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Movement/Linear", fileName = "LinearMovement")]
    public class LinearMovementConfig : MovementConfig
    {
        [SerializeField, Min(0f)] private float speed = 2.5f;
        [SerializeField, Min(0f)] private float acceleration = 20f;

        public override IMovement Create() => new LinearMovement(speed, acceleration);
    }
}