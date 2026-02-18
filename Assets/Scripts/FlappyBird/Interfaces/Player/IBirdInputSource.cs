namespace FlappyBird.Interfaces.Player
{
    /// <summary>
    /// 새 입력을 전달하는 interface입니다.
    /// </summary>
    public interface IBirdInputSource
    {
        // 상승 입력 유지 여부
        bool IsHolding { get; }
    }
}
