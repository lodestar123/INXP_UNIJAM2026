namespace FlappyBird.Interfaces.Game
{
    /// <summary>
    /// 게임 시작, 종료, 점수, 아이템 처리를 다루는 game flow interface입니다.
    /// </summary>
    public interface IFlappyBirdGameFlow
    {
        bool IsPlaying { get; }
        void StartGame();
        void EndGame();
        void TransitionToNextGame();
        void IncrementScore();
        void OnItemCollected(Item item);
    }
}
