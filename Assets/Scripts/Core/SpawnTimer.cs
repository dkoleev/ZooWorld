namespace ZooWorld.Core
{
    public class SpawnTimer
    {
        private readonly IRandom _random;
        private readonly float _minInterval;
        private readonly float _maxInterval;
        private float _timeLeft;

        public SpawnTimer(IRandom random, float minInterval, float maxInterval)
        {
            _random = random;
            _minInterval = minInterval;
            _maxInterval = maxInterval;
            _timeLeft = NextInterval();
        }

        /// <summary>Returns true when one animal should spawn on this tick.</summary>
        public bool Tick(float deltaTime)
        {
            _timeLeft -= deltaTime;
            if (_timeLeft > 0f)
                return false;

            // Adding instead of assigning keeps the average rate right when a frame runs long.
            _timeLeft += NextInterval();
            return true;
        }

        private float NextInterval() => _random.Range(_minInterval, _maxInterval);

    }
}
