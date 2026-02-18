using FlappyBird.Configs;

namespace FlappyBird.Interfaces.Pipes
{
    public interface IPipePatternGenerator
    {
        void Reset(FlappyBirdConfig config);
        PipePatternResult Next(FlappyBirdConfig config);
    }

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
