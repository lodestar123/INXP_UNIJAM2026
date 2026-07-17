using UnityEngine;
using Utils;

namespace Pacman
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PacmanPlayerInput))]
    public class PacmanPlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PacmanPlayerInput input;

        [Header("Movement")]
        [SerializeField] private float speed = 4f;
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private Vector2 initialDirection = Vector2.zero;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Obstacle Check")]
        [SerializeField] private Vector2 obstacleBoxSize = Vector2.one * 0.75f;
        [SerializeField] private float obstacleDistance = 1.5f;

        private Rigidbody2D _rigidbody2D;
        private Vector2 _direction;
        private Vector2 _nextDirection;
        private Vector2 _startingPosition;
        private bool _hasStartingPosition;

        public Rigidbody2D Rigidbody2D => _rigidbody2D;
        public Vector2 CurrentDirection => _direction;
        public Vector2 NextDirection => _nextDirection;

        public Vector2 StartingPosition
        {
            get => _startingPosition;
            set
            {
                _startingPosition = value;
                _hasStartingPosition = true;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
            CaptureStartingPosition();
        }

        private void Start()
        {
            ResetState();
        }

        private void LateUpdate()
        {
            if (IsGameStopped())
            {
                return;
            }

            ReadRequestedDirection();

            if (_nextDirection != Vector2.zero)
            {
                SetDirection(_nextDirection);
            }
        }

        private void FixedUpdate()
        {
            if (_rigidbody2D == null || IsGameStopped())
            {
                return;
            }

            Vector2 position = _rigidbody2D.position;
            Vector2 translation = speed * speedMultiplier * Time.fixedDeltaTime * _direction;
            _rigidbody2D.MovePosition(position + translation);
        }

        public void ResetState()
        {
            EnsureInitialized();
            CaptureStartingPosition();

            speedMultiplier = 1f;
            _direction = initialDirection;
            _nextDirection = Vector2.zero;
            enabled = true;

            if (_rigidbody2D == null)
            {
                return;
            }

            transform.position = new Vector3(_startingPosition.x, _startingPosition.y, transform.position.z);
            _rigidbody2D.position = _startingPosition;
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        }

        public void StopMovement()
        {
            _direction = Vector2.zero;
            _nextDirection = Vector2.zero;

            if (_rigidbody2D == null)
            {
                return;
            }

            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
        }

        public void SetDirection(Vector2 direction, bool forced = false)
        {
            direction = ToCardinal(direction);
            if (direction == Vector2.zero)
            {
                return;
            }

            if (forced || !Occupied(direction))
            {
                _direction = direction;
                _nextDirection = Vector2.zero;
            }
            else
            {
                _nextDirection = direction;
            }
        }

        public bool Occupied(Vector2 direction)
        {
            direction = ToCardinal(direction);
            if (direction == Vector2.zero)
            {
                return false;
            }

            RaycastHit2D hit = Physics2D.BoxCast(
                transform.position,
                obstacleBoxSize,
                0f,
                direction,
                obstacleDistance,
                obstacleLayer);

            return hit.collider != null;
        }

        private void ReadRequestedDirection()
        {
            if (input == null || input.RequestedDirection == Vector2.zero)
            {
                return;
            }

            SetDirection(input.RequestedDirection);
            input.ClearRequestedDirection();
        }

        private void EnsureInitialized()
        {
            if (_rigidbody2D == null)
            {
                _rigidbody2D = GetComponent<Rigidbody2D>();
            }

            if (input == null)
            {
                input = GetComponent<PacmanPlayerInput>();
            }

            if (obstacleLayer.value == 0)
            {
                obstacleLayer = LayerMask.GetMask("Wall");
            }

            if (_rigidbody2D == null)
            {
                INXPLog.Warn("[PacmanPlayerMovement] Rigidbody2D is missing.", this);
            }
        }

        private void CaptureStartingPosition()
        {
            if (_hasStartingPosition)
            {
                return;
            }

            _startingPosition = transform.position;
            _hasStartingPosition = true;
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

        private bool IsGameStopped()
        {
            return GameSceneManager.Instance != null &&
                   (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver);
        }
    }
}
