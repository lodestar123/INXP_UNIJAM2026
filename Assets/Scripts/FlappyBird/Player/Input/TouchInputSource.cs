using FlappyBird.Interfaces.Player;
using UnityEngine;
using UnityEngine.InputSystem; // Import the new Input System namespace

namespace FlappyBird.Player.Input
{
    public sealed class TouchInputSource : MonoBehaviour, IBirdInputSource
    {
        public bool IsHolding => Pointer.current != null && Pointer.current.press.isPressed;
    }
}
