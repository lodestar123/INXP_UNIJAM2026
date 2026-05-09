using FlappyBird.Interfaces.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FlappyBird.Player.Input
{
    /// <summary>
    /// 포인터 입력 상태를 홀드 입력으로 변환합니다.
    /// </summary>
    public sealed class TouchInputSource : MonoBehaviour, IBirdInputSource
    {
        public bool IsHolding => Pointer.current != null && Pointer.current.press.isPressed;
        public bool WasPressedThisFrame => Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        public bool WasReleasedThisFrame => Pointer.current != null && Pointer.current.press.wasReleasedThisFrame;
    }
}
