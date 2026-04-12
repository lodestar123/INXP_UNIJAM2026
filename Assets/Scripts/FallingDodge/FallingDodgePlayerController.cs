using Core.Input;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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
        private float _keyboardDirection;
        private int _activeTouchId = -1;
        private float _touchDirection;

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
            _keyboardDirection = 0f;
            ResetTouchDirection();
        }

        public void ResetState()
        {
            _canMove = true;
            _keyboardDirection = 0f;
            ResetTouchDirection();
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
                float keyboardDirection = ReadKeyboardDirection();
                if (!Mathf.Approximately(keyboardDirection, 0f))
                {
                    return keyboardDirection;
                }
            }

            float touchDirection = ReadTouchDirection();
            if (!Mathf.Approximately(touchDirection, 0f))
            {
                return touchDirection;
            }

            if (UnifiedInputManager.Instance == null || !UnifiedInputManager.Instance.IsPressing)
            {
                return 0f;
            }

            return UnifiedInputManager.Instance.PointerPosition.x < Screen.width * 0.5f ? -1f : 1f;
        }

        private float ReadKeyboardDirection()
        {
            Keyboard keyboard = Keyboard.current;
            bool isLeftPressed = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool isRightPressed = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            bool wasLeftPressedThisFrame = keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame;
            bool wasRightPressedThisFrame = keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame;

            if (wasLeftPressedThisFrame && !wasRightPressedThisFrame)
            {
                _keyboardDirection = -1f;
            }
            else if (wasRightPressedThisFrame && !wasLeftPressedThisFrame)
            {
                _keyboardDirection = 1f;
            }

            if (!isLeftPressed && !isRightPressed)
            {
                _keyboardDirection = 0f;
            }
            else if (_keyboardDirection < 0f && !isLeftPressed)
            {
                _keyboardDirection = isRightPressed ? 1f : 0f;
            }
            else if (_keyboardDirection > 0f && !isRightPressed)
            {
                _keyboardDirection = isLeftPressed ? -1f : 0f;
            }
            else if (Mathf.Approximately(_keyboardDirection, 0f))
            {
                _keyboardDirection = isRightPressed ? 1f : -1f;
            }

            return _keyboardDirection;
        }

        private float ReadTouchDirection()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                ResetTouchDirection();
                return 0f;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    _activeTouchId = touch.touchId.ReadValue();
                    _touchDirection = GetDirectionFromScreenX(touch.position.ReadValue().x);
                }
            }

            if (_activeTouchId == -1)
            {
                return 0f;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.touchId.ReadValue() != _activeTouchId)
                {
                    continue;
                }

                if (touch.press.isPressed)
                {
                    _touchDirection = GetDirectionFromScreenX(touch.position.ReadValue().x);
                    return _touchDirection;
                }

                ResetTouchDirection();
                return 0f;
            }

            ResetTouchDirection();
            return 0f;
        }

        private float GetDirectionFromScreenX(float screenX)
        {
            return screenX < Screen.width * 0.5f ? -1f : 1f;
        }

        private void ResetTouchDirection()
        {
            _activeTouchId = -1;
            _touchDirection = 0f;
        }
    }
}
