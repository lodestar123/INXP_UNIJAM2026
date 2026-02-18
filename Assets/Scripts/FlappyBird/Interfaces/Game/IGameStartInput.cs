namespace FlappyBird.Interfaces.Game
{
    /// <summary>
    /// 게임 시작 입력 여부를 제공하는 input interface입니다.
    /// </summary>
    public interface IGameStartInput
    {
        bool IsStartPressedThisFrame { get; }
    }
}
