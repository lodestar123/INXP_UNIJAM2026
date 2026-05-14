using FlappyBird.Configs;
using FlappyBird.Interfaces.Player;
using UnityEngine;

namespace FlappyBird.Player
{
    /// <summary>
    /// 입력에 따라 플레이어 상승 힘과 속도 제한을 적용합니다.
    /// </summary>
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

        public void MotorFixedTick(bool isHolding, bool wasPressed, bool wasReleased)
        {
            if (!_initialized || flappyBirdConfig is null) return;

            if (wasPressed) // 버튼이 처음 눌렸을 때
            {
                Vector2 velocity = _rigidBody2D.linearVelocity;
                float startY = Mathf.Max(velocity.y, 0.0f);
                _rigidBody2D.linearVelocity = new Vector2(velocity.x, startY);
                _rigidBody2D.AddForce(Vector2.up * flappyBirdConfig.PressImpulse, ForceMode2D.Impulse);
            }

            if (isHolding) // 버튼이 눌린 상태
            {
                Vector2 force = Vector2.up * flappyBirdConfig.HoldForce;
                _rigidBody2D.AddForce(force, ForceMode2D.Force);
            }

            if (wasReleased) // 버튼이 떼어졌을 때
            {
                Vector2 velocity = _rigidBody2D.linearVelocity;

                if (velocity.y > 0.0f)
                {
                    velocity.y *= flappyBirdConfig.ReleaseUpVelocityMultiplier;
                    _rigidBody2D.linearVelocity = velocity;
                }

                if (flappyBirdConfig.ReleaseDownImpulse > 0.0f)
                {
                    _rigidBody2D.AddForce(Vector2.down * flappyBirdConfig.ReleaseDownImpulse, ForceMode2D.Impulse);
                }
            }

            Vector2 clampedVelocity = _rigidBody2D.linearVelocity;
            float clampedY = Mathf.Clamp(clampedVelocity.y, -flappyBirdConfig.MaxDownVelocity, flappyBirdConfig.MaxUpVelocity);
            _rigidBody2D.linearVelocity = new Vector2(clampedVelocity.x, clampedY);
        }

        public void ResetState()
        {
            if (!_initialized) return;

            transform.position = _startPosition;
            _rigidBody2D.linearVelocity = Vector2.zero;
            _rigidBody2D.angularVelocity = 0f;
        }
    }
}
