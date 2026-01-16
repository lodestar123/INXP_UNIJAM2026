namespace FlappyBird.Interfaces.Player
{
    // 플래피 버드 플레이어(모터)가 필요로 하는 입력 소스를 정의하는 인터페이스입니다.
    public interface IBirdInputSource
    {
        // 플레이어가 상승 입력을 유지하고 있는지 여부
        bool IsHolding { get; }
    }
}