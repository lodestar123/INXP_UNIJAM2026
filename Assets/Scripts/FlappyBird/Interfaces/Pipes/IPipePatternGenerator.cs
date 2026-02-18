using FlappyBird.Configs;

namespace FlappyBird.Interfaces.Pipes
{
    /// <summary>
    /// 다음 파이프 패턴(형태, 중심 높이)을 생성합니다.
    /// </summary>
    public interface IPipePatternGenerator
    {
        void Reset(FlappyBirdConfig config);
        PipePatternResult Next(FlappyBirdConfig config);
    }

    /// <summary>
    /// 생성할 파이프 패턴의 결과값입니다.
    /// </summary>
    public readonly struct PipePatternResult
    {
        public PipePatternResult(bool isBranching, float centerY)
        {
            IsBranching = isBranching;
            CenterY = centerY;
        }

        public bool IsBranching { get; }
        public float CenterY { get; }
    }
}
