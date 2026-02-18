namespace FlappyBird.Interfaces.Pipes
{
    /// <summary>
    /// 현재 스크롤 속도와 스크롤 진행 상태를 제공합니다.
    /// </summary>
    public interface IScrollSpeedProvider
    {
        float CurrentScrollSpeed { get; }
        bool IsScrolling { get; }
    }
}
