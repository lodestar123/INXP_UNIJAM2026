namespace FlappyBird.Interfaces.Game
{
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
