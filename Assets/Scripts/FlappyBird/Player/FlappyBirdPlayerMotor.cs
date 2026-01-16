using FlappyBird.Configs;
using FlappyBird.Interfaces.Player;
using UnityEngine;

namespace FlappyBird.Player
{
    public class FlappyBirdPlayerMotor : MonoBehaviour, IFlappyBirdPlayerMotor
    {
        [SerializeField] private FlappyBirdConfig flappyBirdConfig;
        
        private Rigidbody2D _rigidBody2D;
        private Vector2 _startPosition;
        private bool _initialized;

        private void Awake()
        {
            _rigidBody2D = GetComponent<Rigidbody2D>();
            _startPosition = transform.position;
            _initialized = true;
        }

        public void MotorFixedTick(bool isHolding)
        {
            if (!_initialized || flappyBirdConfig == null) return;

            if (isHolding)
            {
                Vector2 force = Vector2.up * flappyBirdConfig.HoldForce;
                _rigidBody2D.AddForce(force, ForceMode2D.Force);
            }
            
            Vector2 velocity = _rigidBody2D.linearVelocity;
            float clampedY = Mathf.Clamp(velocity.y, -flappyBirdConfig.MaxDownVelocity, flappyBirdConfig.MaxDownVelocity);
            _rigidBody2D.linearVelocity = new Vector2(velocity.x, clampedY);
        }

        public void ResetState()
        {
            if(!_initialized) return;
            
            transform.position = _startPosition;
            _rigidBody2D.linearVelocity = Vector2.zero;
            _rigidBody2D.angularVelocity = 0f;
        }
    }
}
