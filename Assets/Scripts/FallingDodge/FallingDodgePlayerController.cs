using Core.Input;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace FallingDodge
{
    public class FallingDodgePlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float minX = -2.8f;
        [SerializeField] private float maxX = 2.8f;
        [SerializeField] private bool allowKeyboardInEditor = true;

        private bool _canMove;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody2D;
        private Collider2D _collider2D;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector3 _initialScale;
        private RigidbodyType2D _initialBodyType;
        private float _initialGravityScale;

        private void Awake()
        {
            _canMove = true;
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _collider2D = GetComponent<Collider2D>();
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _initialScale = transform.localScale;

            if (_rigidbody2D != null)
            {
                _initialBodyType = _rigidbody2D.bodyType;
                _initialGravityScale = _rigidbody2D.gravityScale;
            }
        }

        private void OnEnable()
        {
            _canMove = true;
        }

        public void ResetState()
        {
            _canMove = true;
            transform.DOKill();
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
            transform.localScale = _initialScale;

            if (_rigidbody2D != null)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
                _rigidbody2D.bodyType = _initialBodyType;
                _rigidbody2D.gravityScale = _initialGravityScale;
                _rigidbody2D.simulated = true;
            }

            if (_collider2D != null)
            {
                _collider2D.enabled = true;
            }
        }

        public void StopMovement()
        {
            _canMove = false;
        }

        private void Update()
        {
            if (!_canMove)
            {
                return;
            }

            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            float direction = ReadHorizontalDirection();
            if (Mathf.Approximately(direction, 0f))
            {
                return;
            }

            Vector3 position = transform.position;
            position.x += direction * moveSpeed * Time.deltaTime;
            position.x = Mathf.Clamp(position.x, minX, maxX);
            SpriteFlip(direction);
            transform.position = position;
        }

        private void SpriteFlip(float direction)
        {
            if (direction < 0f)
            {
                _spriteRenderer.flipX = true;
            }
            else
            {
                _spriteRenderer.flipX = false;
            }
        }

        private float ReadHorizontalDirection()
        {
            if (allowKeyboardInEditor && Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                {
                    return -1f;
                }

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                {
                    return 1f;
                }
            }

            if (UnifiedInputManager.Instance == null || !UnifiedInputManager.Instance.IsPressing)
            {
                return 0f;
            }

            return UnifiedInputManager.Instance.PointerPosition.x < Screen.width * 0.5f ? -1f : 1f;
        }
    }
}
