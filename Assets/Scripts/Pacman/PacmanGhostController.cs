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
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
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
        [SerializeField] private bool useClassicScatterCorners = true;

        [Header("Mode Cycle")]
        [SerializeField] private bool cycleScatterAndChase = true;
        [SerializeField] private bool startInScatter = false;
        [SerializeField] private float scatterDuration = 3f;
        [SerializeField] private float chaseDuration = 24f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float centerReachDistance = 0.015f;
        [SerializeField] private bool centerBetweenTwoCellCorridors = true;
        [SerializeField] private int walkableLookAheadCells = 2;
        [SerializeField] private int recentCellAvoidanceCount = 8;

        [Header("Player Catch")]
        [SerializeField] private bool catchPlayerOnTouch = true;

        private readonly List<Vector2Int> _availableDirections = new List<Vector2Int>(4);
        private readonly List<Vector2Int> _candidateDirections = new List<Vector2Int>(4);
        private readonly List<Vector3Int> _recentCells = new List<Vector3Int>(8);

        private Vector3Int _currentCell;
        private Vector3Int _targetCell;
        private Vector2Int _currentDirection;
        private Vector3 _initialLocalPosition;
        private Quaternion _initialLocalRotation;
        private Rigidbody2D _rigidbody2D;
        private bool _isInitialized;
        private bool _hasCaughtPlayer;
        private bool _hasInitialTransform;
        private float _modeTimer;

        // 다른 유령이 읽는 현재 셀/방향.
        public Vector3Int CurrentCell => _currentCell;
        public Vector2Int CurrentDirection => _currentDirection;
        public PacmanGhostMode Mode => mode;

        private void Awake()
        {
            EnsureRigidbody();
            CaptureInitialTransform();
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResetState();
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

            TickModeCycle();
        }

        private void FixedUpdate()
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCatchPlayer(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryCatchPlayer(collision.collider);
        }

        public void ResetState()
        {
            EnsureRigidbody();
            CaptureInitialTransform();

            transform.localPosition = _initialLocalPosition;
            transform.localRotation = _initialLocalRotation;

            if (_rigidbody2D != null)
            {
                _rigidbody2D.position = transform.position;
                _rigidbody2D.linearVelocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0f;
            }

            _hasCaughtPlayer = false;
            _isInitialized = false;
            _currentDirection = Vector2Int.zero;
            _currentCell = Vector3Int.zero;
            _targetCell = Vector3Int.zero;
            _recentCells.Clear();

            if (cycleScatterAndChase)
            {
                mode = startInScatter ? PacmanGhostMode.Scatter : PacmanGhostMode.Chase;
            }

            _modeTimer = GetModeDuration(mode);

            InitializeMovement();
        }

        public void SetMode(PacmanGhostMode nextMode)
        {
            if (mode == nextMode)
            {
                return;
            }

            mode = nextMode;

            if (_isInitialized && _currentDirection != Vector2Int.zero)
            {
                // 모드 변경 시 반대 방향 U턴 없이 새 목표 기준으로 방향을 다시 고름.
                _currentDirection = ChooseDirection(_currentCell, _currentDirection);
                _targetCell = _currentCell + PacmanGrid.ToCellOffset(_currentDirection);
            }
        }

        private void TickModeCycle()
        {
            if (!cycleScatterAndChase)
            {
                return;
            }

            _modeTimer -= Time.deltaTime;
            if (_modeTimer > 0f)
            {
                return;
            }

            PacmanGhostMode nextMode = mode == PacmanGhostMode.Scatter
                ? PacmanGhostMode.Chase
                : PacmanGhostMode.Scatter;

            SetMode(nextMode);
            _modeTimer = GetModeDuration(nextMode);
        }

        private float GetModeDuration(PacmanGhostMode ghostMode)
        {
            float duration = ghostMode == PacmanGhostMode.Scatter ? scatterDuration : chaseDuration;
            return Mathf.Max(0.1f, duration);
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

        private void EnsureRigidbody()
        {
            if (_rigidbody2D == null)
            {
                _rigidbody2D = GetComponent<Rigidbody2D>();
            }
        }

        private void CaptureInitialTransform()
        {
            if (_hasInitialTransform)
            {
                return;
            }

            _initialLocalPosition = transform.localPosition;
            _initialLocalRotation = transform.localRotation;
            _hasInitialTransform = true;
        }

        private void TryCatchPlayer(Collider2D other)
        {
            if (!catchPlayerOnTouch || _hasCaughtPlayer || IsGameStopped())
            {
                return;
            }

            PacmanPlayerController caughtPlayer = other.GetComponentInParent<PacmanPlayerController>();
            if (caughtPlayer == null)
            {
                return;
            }

            _hasCaughtPlayer = true;
            caughtPlayer.StopMovement();

            if (GameSceneManager.Instance == null)
            {
                Debug.LogWarning("[PacmanGhostController] GameSceneManager is missing. Cannot change to Anipang.", this);
                return;
            }

            if (GameSceneManager.Instance.CurrentGameId != 1)
            {
                Debug.LogWarning("[PacmanGhostController] Current game is not Pacman/Past. ChangeGame skipped.", this);
                return;
            }

            GameSceneManager.Instance.OnChangeGame();
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
                if (_rigidbody2D != null)
                {
                    _rigidbody2D.position = transform.position;
                }
            }

            _currentDirection = initialDirection;
            if (_currentDirection == Vector2Int.zero || !CanMove(_currentCell, _currentDirection))
            {
                _currentDirection = ChooseDirection(_currentCell, Vector2Int.zero);
            }

            if (snapToCellCenterOnEnable)
            {
                Vector3 startPosition = GetMovementWorldPosition(_currentCell, _currentDirection);
                transform.position = startPosition;
                if (_rigidbody2D != null)
                {
                    _rigidbody2D.position = startPosition;
                }
            }

            _targetCell = _currentCell + PacmanGrid.ToCellOffset(_currentDirection);
            _isInitialized = true;
        }

        private void MoveToTargetCell()
        {
            EnsureRigidbody();

            if (_rigidbody2D == null)
            {
                return;
            }

            Vector2 targetPosition = GetMovementWorldPosition(_targetCell, _currentDirection);
            Vector2 nextPosition = Vector2.MoveTowards(
                _rigidbody2D.position,
                targetPosition,
                moveSpeed * Time.fixedDeltaTime);

            _rigidbody2D.MovePosition(nextPosition);

            if ((nextPosition - targetPosition).sqrMagnitude > centerReachDistance * centerReachDistance)
            {
                return;
            }

            _rigidbody2D.MovePosition(targetPosition);
            _currentCell = _targetCell;
            RecordRecentCell(_currentCell);
            // 셀 중앙 도착 시에만 다음 방향 결정함.
            _currentDirection = ChooseDirection(_currentCell, _currentDirection);
            _targetCell = _currentCell + PacmanGrid.ToCellOffset(_currentDirection);
        }

        private Vector3 GetMovementWorldPosition(Vector3Int cell, Vector2Int direction)
        {
            Vector3 cellCenter = pacmanGrid.CellToWorldCenter(cell);
            if (!centerBetweenTwoCellCorridors || direction == Vector2Int.zero)
            {
                return cellCenter;
            }

            Vector2Int sideA;
            Vector2Int sideB;
            if (direction.x != 0)
            {
                sideA = Vector2Int.up;
                sideB = Vector2Int.down;
            }
            else
            {
                sideA = Vector2Int.left;
                sideB = Vector2Int.right;
            }

            bool canUseSideA = pacmanGrid.IsWalkable(cell + PacmanGrid.ToCellOffset(sideA));
            bool canUseSideB = pacmanGrid.IsWalkable(cell + PacmanGrid.ToCellOffset(sideB));
            if (canUseSideA == canUseSideB)
            {
                return cellCenter;
            }

            Vector2Int openSide = canUseSideA ? sideA : sideB;
            Vector3 pairedCellCenter = pacmanGrid.CellToWorldCenter(cell + PacmanGrid.ToCellOffset(openSide));
            return (cellCenter + pairedCellCenter) * 0.5f;
        }

        /// <summary>
        /// 이동 가능한 방향 중 목표 셀에 가장 가까워지는 방향 선택함.
        /// 막다른 길이 아니면 바로 뒤돌아가는 방향 제외함.
        /// </summary>
        private Vector2Int ChooseDirection(Vector3Int cell, Vector2Int currentDirection)
        {
            GetLookAheadWalkableDirections(cell, _availableDirections);
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
            bool hasFreshCandidate = HasFreshCandidate(cell);

            for (int i = 0; i < _candidateDirections.Count; i++)
            {
                Vector2Int direction = _candidateDirections[i];
                Vector3Int nextCell = cell + PacmanGrid.ToCellOffset(direction);
                if (hasFreshCandidate && IsRecentlyVisited(nextCell))
                {
                    continue;
                }

                int distance = SquaredCellDistance(nextCell, target);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        private bool HasFreshCandidate(Vector3Int cell)
        {
            for (int i = 0; i < _candidateDirections.Count; i++)
            {
                Vector3Int nextCell = cell + PacmanGrid.ToCellOffset(_candidateDirections[i]);
                if (!IsRecentlyVisited(nextCell))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsRecentlyVisited(Vector3Int cell)
        {
            for (int i = 0; i < _recentCells.Count; i++)
            {
                if (_recentCells[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }

        private void RecordRecentCell(Vector3Int cell)
        {
            if (recentCellAvoidanceCount <= 0)
            {
                _recentCells.Clear();
                return;
            }

            _recentCells.Remove(cell);
            _recentCells.Add(cell);

            while (_recentCells.Count > recentCellAvoidanceCount)
            {
                _recentCells.RemoveAt(0);
            }
        }

        private int GetLookAheadWalkableDirections(Vector3Int cell, List<Vector2Int> results)
        {
            results.Clear();

            int lookAheadCells = Mathf.Max(1, walkableLookAheadCells);
            AddWalkableDirections(cell, lookAheadCells, results);

            if (results.Count == 0 && lookAheadCells > 1)
            {
                AddWalkableDirections(cell, 1, results);
            }

            return results.Count;
        }

        private void AddWalkableDirections(Vector3Int cell, int lookAheadCells, List<Vector2Int> results)
        {
            for (int i = 0; i < PacmanGrid.DirectionOrder.Length; i++)
            {
                Vector2Int direction = PacmanGrid.DirectionOrder[i];
                if (CanMove(cell, direction, lookAheadCells))
                {
                    results.Add(direction);
                }
            }
        }

        private Vector3Int GetTargetCell()
        {
            if (mode == PacmanGhostMode.Scatter)
            {
                // INXPLog.Info($"[PacmanGhostController] {ghostType} is scattering to {scatterTargetCell}");
                return GetScatterTargetCell();
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
                    return SquaredCellDistance(_currentCell, playerCell) > 64 ? playerCell : GetScatterTargetCell();

                default:
                    // Blinky: 플레이어 현재 셀 직접 추적함.
                    return playerCell;
            }
        }

        private Vector3Int GetScatterTargetCell()
        {
            if (!useClassicScatterCorners || pacmanGrid == null || pacmanGrid.WallTilemap == null)
            {
                return scatterTargetCell;
            }

            BoundsInt bounds = pacmanGrid.WallTilemap.cellBounds;
            if (bounds.size.x <= 0 || bounds.size.y <= 0)
            {
                return scatterTargetCell;
            }

            int left = bounds.xMin;
            int right = bounds.xMax - 1;
            int bottom = bounds.yMin;
            int top = bounds.yMax - 1;
            Vector3Int desiredCorner;

            switch (ghostType)
            {
                case PacmanGhostType.Pinky:
                    desiredCorner = new Vector3Int(left, top, 0);
                    break;

                case PacmanGhostType.Inky:
                    desiredCorner = new Vector3Int(right, bottom, 0);
                    break;

                case PacmanGhostType.Clyde:
                    desiredCorner = new Vector3Int(left, bottom, 0);
                    break;

                default:
                    desiredCorner = new Vector3Int(right, top, 0);
                    break;
            }

            return FindNearestWalkableCell(desiredCorner, bounds);
        }

        private Vector3Int FindNearestWalkableCell(Vector3Int desiredCell, BoundsInt bounds)
        {
            Vector3Int bestCell = scatterTargetCell;
            int bestDistance = int.MaxValue;
            bool foundWalkableCell = false;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    if (!pacmanGrid.IsWalkable(cell))
                    {
                        continue;
                    }

                    int distance = SquaredCellDistance(cell, desiredCell);
                    if (distance >= bestDistance)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    bestCell = cell;
                    foundWalkableCell = true;
                }
            }

            return foundWalkableCell ? bestCell : scatterTargetCell;
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
            return CanMove(cell, direction, 1);
        }

        private bool CanMove(Vector3Int cell, Vector2Int direction, int cellsToCheck)
        {
            if (direction == Vector2Int.zero)
            {
                return false;
            }

            Vector3Int offset = PacmanGrid.ToCellOffset(direction);
            int clampedCellsToCheck = Mathf.Max(1, cellsToCheck);
            for (int i = 1; i <= clampedCellsToCheck; i++)
            {
                if (!pacmanGrid.IsWalkable(cell + offset * i))
                {
                    return false;
                }
            }

            return true;
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
