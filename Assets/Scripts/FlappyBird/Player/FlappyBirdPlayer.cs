using FlappyBird.Game;
using FlappyBird.Interfaces.Player;
using UnityEngine;

namespace FlappyBird.Player
{
    /// <summary>
    /// Connects player input and motor, and handles collision detection.
    /// This is the main controller component for the player GameObject.
    /// </summary>
    [RequireComponent(typeof(IFlappyBirdPlayerMotor))]
    [RequireComponent(typeof(IBirdInputSource))]
    public class FlappyBirdPlayer : MonoBehaviour
    {
        private IFlappyBirdPlayerMotor _motor;
        private IBirdInputSource _inputSource;

        private bool _isPlayerActive = false;

        private void Awake()
        {
            // Get references to the motor and input source components.
            _motor = GetComponent<IFlappyBirdPlayerMotor>();
            _inputSource = GetComponent<IBirdInputSource>();
        }

        private void FixedUpdate()
        {
            // Only process motor logic if the player is active and input is available.
            if (!_isPlayerActive || _inputSource == null)
            {
                return;
            }

            _motor.MotorFixedTick(_inputSource.IsHolding);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // If the player is active and collides, trigger game over.
            if (_isPlayerActive)
            {
                // Notify the GameManager that the player has died.
                // Note: We will create the GameManager in the next step.
                FlappyBirdGameManager.Instance.EndGame(); 
                
                // Deactivate the player to prevent further actions.
                DeactivatePlayer();
            }
        }

        /// <summary>
        /// Activates the player, enabling movement and collision.
        /// To be called by the GameManager when the game starts.
        /// </summary>
        public void ActivatePlayer()
        {
            _isPlayerActive = true;
        }

        /// <summary>
        /// Deactivates the player, stopping movement processing.
        /// </summary>
        public void DeactivatePlayer()
        {
            _isPlayerActive = false;
        }
        
        /// <summary>
        /// Resets the player to its initial state via the motor.
        /// To be called by the GameManager.
        /// </summary>
        public void ResetPlayer()
        {
            _motor.ResetState();
            // The player is not activated here; GameManager will activate it when the game starts.
            DeactivatePlayer();
        }
    }
}
