using UnityEngine;
using ZooWorld.Core.Movement;

namespace ZooWorld.Game.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Movement/Jump", fileName = "JumpMovement")]
    public class JumpMovementConfig : MovementConfig
    {
        [SerializeField, Min(0.1f)] private float interval = 1.5f;
        [SerializeField, Min(0f)] private float distance = 2.5f;

        public override IMovement Create() => new JumpMovement(interval, distance, Physics.gravity.magnitude);
    }
}