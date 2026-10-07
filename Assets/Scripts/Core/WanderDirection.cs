using UnityEngine;
using ZooWorld.Core.World;

namespace ZooWorld.Core
{
    public class WanderDirection
    {
        private readonly IPlayArea _area;
        private readonly IRandom _random;
        private readonly float _interval;
        private Vector3 _current;
        private float _timeLeft;

        public WanderDirection(IPlayArea area, IRandom random, float interval)
        {
            _area = area;
            _random = random;
            _interval = interval;
            Reroll();
        }

        public Vector3 Tick(Vector3 position, float deltaTime)
        {
            if (!_area.Contains(position))
            {
                var toCentre = _area.Center - position;
                toCentre.y = 0f;
                _current = toCentre.normalized;
                // Restarting the timer keeps the animal walking inward for a while after it is back.
                _timeLeft = _interval;
                
                return _current;
            }

            _timeLeft -= deltaTime;
            if (_timeLeft <= 0f)
                Reroll();
            
            return _current;
        }

        private void Reroll()
        {
            //generating a random angle. 2f * Mathf.PI = 6.28318 represents a full $360 circle.
            var angle = _random.Range(0f, 2f * Mathf.PI);
            _current = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            _timeLeft = _interval;
        }
    }
}