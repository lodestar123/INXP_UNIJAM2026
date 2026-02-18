namespace FlappyBird.Game
{
    /// <summary>
    /// 플래피버드 게임의 상태를 나타냅니다.
    /// </summary>
    public enum FlappyBirdState
    {
        Ready,
        Playing,
        GameOver
    }

    /// <summary>
    /// Ready, Playing, GameOver state 전환을 처리합니다.
    /// </summary>
    public sealed class FlappyBirdStateMachine
    {
        public FlappyBirdState Current { get; private set; } = FlappyBirdState.Ready;

        public bool Is(FlappyBirdState state)
        {
            return Current == state;
        }

        public void Set(FlappyBirdState state)
        {
            Current = state;
        }

        public bool TryStart()
        {
            if (Current != FlappyBirdState.Ready)
            {
                return false;
            }

            Current = FlappyBirdState.Playing;
            return true;
        }

        public bool TryEnd()
        {
            if (Current != FlappyBirdState.Playing)
            {
                return false;
            }

            Current = FlappyBirdState.GameOver;
            return true;
        }
    }
}
