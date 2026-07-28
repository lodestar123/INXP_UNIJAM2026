using UnityEngine;

namespace Pacman
{
    /// <summary>
    /// Stage3 팩맨 플레이어의 외부 참조용 진입점.
    /// 입력과 이동 처리는 각각 PacmanPlayerInput, PacmanPlayerMovement가 담당함.
    /// </summary>
    [RequireComponent(typeof(PacmanPlayerInput))]
    [RequireComponent(typeof(PacmanPlayerMovement))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PacmanPlayerController : MonoBehaviour
    {
        private PacmanPlayerInput _input;
        private PacmanPlayerMovement _movement;

        public Rigidbody2D Rigidbody2D => _movement != null ? _movement.Rigidbody2D : null;

        public Vector2 StartingPosition
        {
            get => _movement != null ? _movement.StartingPosition : (Vector2)transform.position;
            set
            {
                EnsureReferences();
                if (_movement != null)
                {
                    _movement.StartingPosition = value;
                }
            }
        }

        // 유령 AI가 읽는 현재 진행 방향.
        public Vector2 CurrentDirection => _movement != null ? _movement.CurrentDirection : Vector2.zero;

        public void Configure(PacmanConfig config)
        {
            EnsureReferences();
            _input?.Configure(config);
            _movement?.Configure(config);
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnEnable()
        {
            ResetState();
        }

        public void ResetState()
        {
            EnsureReferences();
            _input?.ResetState();
            _movement?.ResetState();
        }

        /// <summary>
        /// 외부 게임 흐름에서 플레이어를 즉시 멈출 때 사용함.
        /// </summary>
        public void StopMovement()
        {
            EnsureReferences();
            _input?.StopReadingInput();
            _movement?.StopMovement();
        }

        private void EnsureReferences()
        {
            if (_input == null)
            {
                _input = GetComponent<PacmanPlayerInput>();
            }

            if (_movement == null)
            {
                _movement = GetComponent<PacmanPlayerMovement>();
            }
        }
    }
}
