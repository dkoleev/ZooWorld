using UnityEngine;

namespace ZooWorld.Core.Movement
{
    public class JumpMovement : IMovement
    {
        // sin(45°) = cos(45°): the launch splits evenly between forward and up.
        private const float Diagonal = 0.70710678f;

        private readonly float _interval;
        private readonly float _launchSpeed;
        private float _timeLeft;

        public JumpMovement(float interval, float distance, float gravity)
        {
            _interval = interval;
            // A 45° launch from flat ground lands v² / g away, so v = sqrt(distance * g).
            _launchSpeed = Mathf.Sqrt(Mathf.Max(0f, distance * gravity));
            _timeLeft = interval;
        }

        public void Tick(IEntityBody target, Vector3 direction, float deltaTime)
        {
            _timeLeft -= deltaTime;
            if (_timeLeft > 0f || !target.IsGrounded)
                return;

            _timeLeft = _interval;
            // Replacing the velocity, not adding to it, keeps every hop the same length even when
            // the frog is still sliding from its last landing or from a shove.
            var launch = (direction + Vector3.up) * (_launchSpeed * Diagonal);
            target.AddVelocity(launch - target.Velocity);
        }
    }
}
