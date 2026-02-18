namespace FlappyBird
{
    public sealed class DistanceSpawnScheduler
    {
        private float _movedDistance;

        public void Reset()
        {
            _movedDistance = 0f;
        }

        public bool TryConsume(float deltaDistance, float spawnIntervalDistance)
        {
            if (spawnIntervalDistance <= 0f)
            {
                return false;
            }

            _movedDistance += deltaDistance;

            if (_movedDistance < spawnIntervalDistance)
            {
                return false;
            }

            _movedDistance -= spawnIntervalDistance;
            return true;
        }
    }
}
