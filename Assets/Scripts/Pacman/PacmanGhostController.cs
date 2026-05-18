using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Pacman
{
    /// <summary>
    /// 팩맨 원작의 유령별 타겟 계산 방식 구분함.
    /// </summary>
    public enum PacmanGhostType
    {
        Blinky,
        Pinky,
        Inky,
        Clyde,
    }

    public enum PacmanGhostMode
    {
        // 각 유령이 지정된 코너/셀로 향하는 모드.
        Scatter,
        // 유령 종류별 타겟 셀을 계산해 플레이어를 압박하는 모드.
        Chase,
    }

    /// <summary>
    /// Tilemap 셀 중심을 따라 움직이는 팩맨 유령 AI.
    /// 교차점마다 목표 셀에 가장 가까워지는 방향을 선택함.
    /// </summary>
    public class PacmanGhostController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PacmanGrid pacmanGrid;
        [SerializeField] private Transform player;
        [SerializeField] private PacmanPlayerController playerController;
        // Inky 타겟 계산에 사용하는 Blinky 참조.
        [SerializeField] private PacmanGhostController blinky;

        [Header("Ghost")]
        [SerializeField] private PacmanGhostType ghostType = PacmanGhostType.Blinky;
        [SerializeField] private PacmanGhostMode mode = PacmanGhostMode.Chase;
        // Scatter 모드 또는 Clyde 근접 시 향할 셀.
        [SerializeField] private Vector3Int scatterTargetCell = new Vector3Int(-20, 20, 0);
        // 시작 시 우선 시도할 방향. 막혀 있으면 자동 선택함.
        [SerializeField] private Vector2Int initialDirection = Vector2Int.left;
        [SerializeField] private bool snapToCellCenterOnEnable = true;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float centerReachDistance = 0.015f;

        private readonly List<Vector2Int> _availableDirections = new List<Vector2Int>(4);
        private readonly List<Vector2Int> _candidateDirections = new List<Vector2Int>(4);

        private Vector3Int _currentCell;
        private Vector3Int _targetCell;
        private Vector2Int _currentDirection;
        private bool _isInitialized;

        // 다른 유령이 읽는 현재 셀/방향.
        public Vector3Int CurrentCell => _currentCell;
        public Vector2Int CurrentDirection => _currentDirection;
        public PacmanGhostMode Mode => mode;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            InitializeMovement();
        }

        private void Update()
        {
            if (IsGameStopped())
            {
                return;
            }

            if (!_isInitialized)
            {
                InitializeMovement();
            }

            if (!_isInitialized)
            {
                return;
            }

            MoveToTargetCell();
        }

        public void SetMode(PacmanGhostMode nextMode)
        {
            if (mode == nextMode)
            {
                return;
            }

            mode = nextMode;

            if (_currentDirection != Vector2Int.zero)
            {
                // 모드 변경 시 즉시 반대 방향으로 전환함.
                _currentDirection = -_currentDirection;
                _targetCell = _currentCell + PacmanGrid.ToCellOffset(_currentDirection);
            }
        }

        private void ResolveReferences()
        {
            if (pacmanGrid == null)
            {
                // 씬의 PacmanGrid 자동 탐색함.
                pacmanGrid = FindFirstObjectByType<PacmanGrid>();
            }

            if (player == null)
            {
                PacmanPlayerController foundPlayer = FindFirstObjectByType<PacmanPlayerController>();
                if (foundPlayer != null)
                {
                    player = foundPlayer.transform;
                    playerController = foundPlayer;
                }
            }
            else if (playerController == null)
            {
                playerController = player.GetComponent<PacmanPlayerController>();
            }
        }

        private void InitializeMovement()
        {
            ResolveReferences();

            if (pacmanGrid == null)
            {
                Debug.LogWarning("[PacmanGhostController] PacmanGrid is missing.", this);
                return;
            }

            _currentCell = pacmanGrid.WorldToCell(transform.position);

            if (snapToCellCenterOnEnable)
            {
                // 시작 위치를 가장 가까운 셀 중앙에 맞춤.
                transform.position = pacmanGrid.CellToWorldCenter(_currentCell);
            }

            _currentDirection = initialDirection;
            if (_currentDirection == Vector2Int.zero || !CanMove(_currentCell, _currentDirection))
            {
                _currentDirection = ChooseDirection(_currentCell, Vector2Int.zero);
            }

            _targetCell = _currentCell + PacmanGrid.ToCellOffset(_currentDirection);
            _isInitialized = true;
        }

        private void MoveToTargetCell()
        {
            Vector3 targetPosition = pacmanGrid.CellToWorldCenter(_targetCell);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if ((transform.position - targetPosition).sqrMagnitude > centerReachDistance * centerReachDistance)
            {
                return;
            }

            transform.position = targetPosition;
            _currentCell = _targetCell;
            // 셀 중앙 도착 시에만 다음 방향 결정함.
            _currentDirection = ChooseDirection(_currentCell, _currentDirection);
            _targetCell = _currentCell + PacmanGrid.ToCellOffset(_currentDirection);
        }

        /// <summary>
        /// 이동 가능한 방향 중 목표 셀에 가장 가까워지는 방향 선택함.
        /// 막다른 길이 아니면 바로 뒤돌아가는 방향 제외함.
        /// </summary>
        private Vector2Int ChooseDirection(Vector3Int cell, Vector2Int currentDirection)
        {
            pacmanGrid.GetWalkableDirections(cell, _availableDirections);
            if (_availableDirections.Count == 0)
            {
                return Vector2Int.zero;
            }

            Vector2Int reverseDirection = -currentDirection;
            _candidateDirections.Clear();

            for (int i = 0; i < _availableDirections.Count; i++)
            {
                Vector2Int direction = _availableDirections[i];
                if (currentDirection == Vector2Int.zero || direction != reverseDirection)
                {
                    _candidateDirections.Add(direction);
                }
            }

            if (_candidateDirections.Count == 0)
            {
                // 막다른 길에서는 뒤돌기 허용함.
                _candidateDirections.AddRange(_availableDirections);
            }

            Vector3Int target = GetTargetCell();
            Vector2Int bestDirection = _candidateDirections[0];
            int bestDistance = int.MaxValue;

            for (int i = 0; i < _candidateDirections.Count; i++)
            {
                Vector2Int direction = _candidateDirections[i];
                Vector3Int nextCell = cell + PacmanGrid.ToCellOffset(direction);
                int distance = SquaredCellDistance(nextCell, target);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        private Vector3Int GetTargetCell()
        {
            if (mode == PacmanGhostMode.Scatter)
            {
                CustomLog.Info($"[PacmanGhostController] {ghostType} is scattering to {scatterTargetCell}");
                return scatterTargetCell;
            }

            Vector3Int playerCell = player != null ? pacmanGrid.WorldToCell(player.position) : _currentCell;
            Vector2Int playerDirection = GetPlayerDirection();

            switch (ghostType)
            {
                case PacmanGhostType.Pinky:
                    // Pinky: 플레이어 진행 방향 4칸 앞을 노림.
                    return playerCell + PacmanGrid.ToCellOffset(playerDirection * 4);

                case PacmanGhostType.Inky:
                    // Inky: 플레이어 앞 2칸과 Blinky 위치로 대칭 타겟 계산함.
                    Vector3Int pivotCell = playerCell + PacmanGrid.ToCellOffset(playerDirection * 2);
                    Vector3Int blinkyCell = blinky != null ? blinky.CurrentCell : _currentCell;
                    return pivotCell + (pivotCell - blinkyCell);

                case PacmanGhostType.Clyde:
                    // Clyde: 멀면 추적, 8셀 이내면 scatterTargetCell로 물러남.
                    return SquaredCellDistance(_currentCell, playerCell) > 64 ? playerCell : scatterTargetCell;

                default:
                    // Blinky: 플레이어 현재 셀 직접 추적함.
                    return playerCell;
            }
        }

        private Vector2Int GetPlayerDirection()
        {
            if (playerController == null)
            {
                return Vector2Int.left;
            }

            Vector2Int direction = PacmanGrid.ToGridDirection(playerController.CurrentDirection);
            return direction == Vector2Int.zero ? Vector2Int.left : direction;
        }

        private bool CanMove(Vector3Int cell, Vector2Int direction)
        {
            return direction != Vector2Int.zero &&
                   pacmanGrid.IsWalkable(cell + PacmanGrid.ToCellOffset(direction));
        }

        private static int SquaredCellDistance(Vector3Int a, Vector3Int b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return dx * dx + dy * dy;
        }

        private bool IsGameStopped()
        {
            return GameSceneManager.Instance != null &&
                   (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver);
        }

        private void OnDrawGizmosSelected()
        {
            if (pacmanGrid == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pacmanGrid.CellToWorldCenter(_targetCell), 0.12f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(pacmanGrid.CellToWorldCenter(GetTargetCell()), 0.18f);
        }
    }
}
