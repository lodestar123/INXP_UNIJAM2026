using FlappyBird.Interfaces.Player;
using UnityEngine;
using UnityEngine.InputSystem; // Import the new Input System namespace

namespace FlappyBird.Player.Input
{
    public sealed class TouchInputSource : MonoBehaviour, IBirdInputSource
    {
        /// <summary>
        /// Gets whether the primary pointer (mouse, touch, pen) is currently being pressed.
        /// </summary>
        public bool IsHolding
        {
            get
            {
                // Pointer.current will be null if no pointer device is active.
                if (Pointer.current == null)
                    return false;

                // isPressed is true if the primary button on the pointer is held down.
                return Pointer.current.press.isPressed;
            }
        }
    }
}
