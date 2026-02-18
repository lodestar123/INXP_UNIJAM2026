using FlappyBird.Configs;
using FlappyBird.Interfaces.Pipes;
using UnityEngine;

namespace FlappyBird
{
    /// <summary>
    /// 기본 규칙으로 다음 파이프 패턴을 생성합니다.
    /// </summary>
    public sealed class DefaultPipePatternGenerator : IPipePatternGenerator
    {
        private float _lastPatternCenterY;
        private bool _wasLastPatternBranching;
        private int _spawnedPatternCount;

        public void Reset(FlappyBirdConfig config)
        {
            _lastPatternCenterY = (config.PipeMinY + config.PipeMaxY) / 2f;
            _wasLastPatternBranching = false;
            _spawnedPatternCount = 0;
        }

        public PipePatternResult Next(FlappyBirdConfig config)
        {
            bool isBranching = Random.value < config.DoublePipeChance;

            if (_wasLastPatternBranching || _spawnedPatternCount == 0)
            {
                isBranching = false;
            }

            _spawnedPatternCount++;

            float nextPatternCenterY = isBranching
                ? (config.PipeMinY + config.PipeMaxY) / 2f
                : CalculateNextSpawnHeight(config);

            _lastPatternCenterY = nextPatternCenterY;
            _wasLastPatternBranching = isBranching;

            return new PipePatternResult(isBranching, nextPatternCenterY);
        }

        private float CalculateNextSpawnHeight(FlappyBirdConfig config)
        {
            float variance = Random.Range(-config.PipeHeightVariance, config.PipeHeightVariance);
            float newY = _lastPatternCenterY + variance;
            return Mathf.Clamp(newY, config.PipeMinY, config.PipeMaxY);
        }
    }
}
