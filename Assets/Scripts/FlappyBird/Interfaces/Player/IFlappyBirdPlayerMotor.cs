using UnityEngine;

namespace FlappyBird.Interfaces.Player
{
    /// <summary>
    /// 플레이어 물리 이동 로직을 수행하는 motor interface입니다.
    /// </summary>
    public interface IFlappyBirdPlayerMotor
    {
        void MotorFixedTick(bool isHolding, bool wasPressed, bool wasReleased);
        void ResetState();
    }
}
