using UnityEngine;

namespace FlappyBird.Interfaces.Player
{
    public interface IFlappyBirdPlayerMotor
    {
        void MotorFixedTick(bool isHolding);
        void ResetState();
    }
}
