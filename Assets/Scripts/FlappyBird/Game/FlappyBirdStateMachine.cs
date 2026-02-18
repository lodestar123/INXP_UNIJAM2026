namespace FlappyBird.Game
{
    public enum FlappyBirdState
    {
        Ready,
        Playing,
        GameOver
    }

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
