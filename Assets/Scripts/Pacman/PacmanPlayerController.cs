using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Pacman
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PacmanPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float bodyRadius = 0.34f;
        [SerializeField] private float wallSkin = 0.03f;
        [SerializeField] private float turnProbeDistance = 0.16f;
        [SerializeField] private LayerMask wallLayerMask = ~0;
        [SerializeField] private bool rotateToMoveDirection = true;
        [SerializeField] private bool forceZeroGravity = true;

        [Header("Input")]
        [SerializeField] private float swipeThreshold = 60f;
        [SerializeField] private bool allowKeyboardInEditor = true;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[8];

        private Rigidbody2D _rigidbody2D;
        private Collider2D _collider2D;
        private RigidbodyType2D _initialBodyType;
        private RigidbodyConstraints2D _initialConstraints;
        private float _initialGravityScale;

        private Vector2 _currentDirection;
        private Vector2 _requestedDirection;
        private Vector2 _touchStartPosition;
        private Vector2 _mouseStartPosition;
        private int _activeTouchId = -1;
        private bool _hasTouchStartPosition;
        private bool _hasMouseStartPosition;
        private bool _canMove = true;
        private bool _isInitialized;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            _canMove = true;
            ResetInputState();
        }

        private void Update()
        {
            if (!_canMove || IsGameStopped())
            {
                return;
            }

            ReadKeyboardInput();
            ReadTouchSwipe();
            ReadMouseSwipe();
        }

        private void FixedUpdate()
        {
            EnsureInitialized();

            if (!_isInitialized || !_canMove || IsGameStopped())
            {
                StopBody();
                return;
            }

            TryApplyRequestedDirection();
            MoveCurrentDirection();
        }

        public void ResetState()
        {
            EnsureInitialized();

            if (!_isInitialized)
            {
                return;
            }

            _canMove = true;
            _currentDirection = Vector2.zero;
            _requestedDirection = Vector2.zero;
            ResetInputState();

            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
            _rigidbody2D.bodyType = _initialBodyType;
            _rigidbody2D.constraints = _initialConstraints | RigidbodyConstraints2D.FreezeRotation;
            _rigidbody2D.gravityScale = forceZeroGravity ? 0f : _initialGravityScale;
            _rigidbody2D.simulated = true;
        }

        public void StopMovement()
        {
            _canMove = false;
            _currentDirection = Vector2.zero;
            _requestedDirection = Vector2.zero;
            StopBody();
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            _rigidbody2D = GetComponent<Rigidbody2D>();
            _collider2D = GetComponent<Collider2D>();

            if (_rigidbody2D == null || _collider2D == null)
            {
                Debug.LogError("[PacmanPlayerController] Rigidbody2D or Collider2D is missing.", this);
                return;
            }

            _initialBodyType = _rigidbody2D.bodyType;
            _initialConstraints = _rigidbody2D.constraints;
            _initialGravityScale = _rigidbody2D.gravityScale;

            _rigidbody2D.gravityScale = forceZeroGravity ? 0f : _rigidbody2D.gravityScale;
            _rigidbody2D.constraints |= RigidbodyConstraints2D.FreezeRotation;
            _isInitialized = true;
        }

        private void MoveCurrentDirection()
        {
            if (_currentDirection == Vector2.zero)
            {
                StopBody();
                return;
            }

            float moveDistance = moveSpeed * Time.fixedDeltaTime;
            float allowedDistance = GetAllowedMoveDistance(_currentDirection, moveDistance);

            if (allowedDistance <= 0f)
            {
                StopBody();
                return;
            }

            Vector2 nextPosition = _rigidbody2D.position + _currentDirection * allowedDistance;
            _rigidbody2D.MovePosition(nextPosition);
            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        private void TryApplyRequestedDirection()
        {
            if (_requestedDirection == Vector2.zero)
            {
                return;
            }

            bool isReverse = _currentDirection != Vector2.zero &&
                             Vector2.Dot(_currentDirection, _requestedDirection) < -0.9f;

            if (isReverse || HasRoom(_requestedDirection, turnProbeDistance))
            {
                _currentDirection = _requestedDirection;
                _requestedDirection = Vector2.zero;
                ApplyRotation(_currentDirection);
            }
        }

        private void RequestDirection(Vector2 direction)
        {
            direction = ToCardinal(direction);

            if (direction == Vector2.zero)
            {
                return;
            }

            _requestedDirection = direction;

            if (_currentDirection == Vector2.zero)
            {
                TryApplyRequestedDirection();
            }
        }

        private float GetAllowedMoveDistance(Vector2 direction, float desiredDistance)
        {
            float castDistance = desiredDistance + wallSkin;
            int hitCount = Physics2D.CircleCastNonAlloc(
                _rigidbody2D.position,
                bodyRadius,
                direction,
                _hits,
                castDistance,
                wallLayerMask);

            float nearestDistance = castDistance;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = _hits[i].collider;
                if (hitCollider == null || hitCollider == _collider2D || hitCollider.isTrigger)
                {
                    continue;
                }

                nearestDistance = Mathf.Min(nearestDistance, _hits[i].distance);
            }

            if (nearestDistance >= castDistance)
            {
                return desiredDistance;
            }

            return Mathf.Max(0f, nearestDistance - wallSkin);
        }

        private bool HasRoom(Vector2 direction, float distance)
        {
            return GetAllowedMoveDistance(direction, distance) >= distance;
        }

        private void ReadKeyboardInput()
        {
            if (!allowKeyboardInEditor || Keyboard.current == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                RequestDirection(Vector2.up);
            }
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                RequestDirection(Vector2.down);
            }
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                RequestDirection(Vector2.left);
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                RequestDirection(Vector2.right);
            }
        }

        private void ReadTouchSwipe()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                ResetTouchInput();
                return;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    _activeTouchId = touch.touchId.ReadValue();
                    _touchStartPosition = touch.position.ReadValue();
                    _hasTouchStartPosition = true;
                    return;
                }
            }

            if (_activeTouchId == -1 || !_hasTouchStartPosition)
            {
                return;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.touchId.ReadValue() != _activeTouchId)
                {
                    continue;
                }

                if (touch.press.isPressed)
                {
                    TryApplySwipe(touch.position.ReadValue() - _touchStartPosition);
                    return;
                }

                ResetTouchInput();
                return;
            }

            ResetTouchInput();
        }

        private void ReadMouseSwipe()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                ResetMouseInput();
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _mouseStartPosition = mouse.position.ReadValue();
                _hasMouseStartPosition = true;
                return;
            }

            if (!_hasMouseStartPosition)
            {
                return;
            }

            if (mouse.leftButton.isPressed)
            {
                TryApplySwipe(mouse.position.ReadValue() - _mouseStartPosition);
                return;
            }

            ResetMouseInput();
        }

        private void TryApplySwipe(Vector2 swipeDelta)
        {
            if (swipeDelta.sqrMagnitude < swipeThreshold * swipeThreshold)
            {
                return;
            }

            RequestDirection(swipeDelta);
            ResetTouchInput();
            ResetMouseInput();
        }

        private void ApplyRotation(Vector2 direction)
        {
            if (!rotateToMoveDirection || direction == Vector2.zero)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private static Vector2 ToCardinal(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                return Vector2.zero;
            }

            return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? new Vector2(Mathf.Sign(direction.x), 0f)
                : new Vector2(0f, Mathf.Sign(direction.y));
        }

        private void StopBody()
        {
            if (_rigidbody2D != null)
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
            }
        }

        private bool IsGameStopped()
        {
            return GameSceneManager.Instance != null &&
                   (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver);
        }

        private void ResetInputState()
        {
            ResetTouchInput();
            ResetMouseInput();
        }

        private void ResetTouchInput()
        {
            _activeTouchId = -1;
            _hasTouchStartPosition = false;
            _touchStartPosition = Vector2.zero;
        }

        private void ResetMouseInput()
        {
            _hasMouseStartPosition = false;
            _mouseStartPosition = Vector2.zero;
        }
    }
}
