using FlappyBird.Interfaces.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FlappyBird.Game
{
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
