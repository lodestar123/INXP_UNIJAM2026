namespace FlappyBird.Interfaces.Pipes
{
    public interface IScrollSpeedProvider
    {
        float CurrentScrollSpeed { get; }
        bool IsScrolling { get; }
    }
}
