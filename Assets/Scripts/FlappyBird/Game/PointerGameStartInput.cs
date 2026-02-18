using FlappyBird.Interfaces.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FlappyBird.Game
{
    /// <summary>
    /// 포인터(터치/마우스) 입력으로 게임 시작 신호를 제공합니다.
    /// </summary>
    public class PointerGameStartInput : MonoBehaviour, IGameStartInput
    {
        public bool IsStartPressedThisFrame
        {
            get
            {
                return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
            }
        }
    }
}
