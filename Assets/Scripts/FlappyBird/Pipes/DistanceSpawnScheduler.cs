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

        /// <summary>
        /// 이 메서드는 누적 이동 거리를 업데이트하고, 스폰 간격 거리(spawnIntervalDistance)를 초과했는지 여부를 반환합니다.
        /// </summary>
        /// <param name="deltaDistance"></param>
        /// <param name="spawnIntervalDistance"></param>
        /// <returns></returns>
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
