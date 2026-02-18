namespace FlappyBird
{
    /// <summary>
    /// 누적 이동 거리를 기준으로 스폰 타이밍을 판정합니다.
    /// </summary>
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
