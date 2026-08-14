using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Pacman
{
    public class PacmanGameManager : MonoBehaviour
    {
        public static PacmanGameManager Instance { get; private set; }

        [SerializeField] private PacmanConfig config;
        [SerializeField] private PacmanPlayerController player;
        [SerializeField] private PacmanGhostController[] ghosts;
        [SerializeField] private PacmanItemSpawner itemSpawner;

        private Vector3[] _ghostSpawnPositions = new Vector3[0];
        private Vector3[] _shuffledGhostSpawnPositions = new Vector3[0];

        private bool _isReadyToStart;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            ResolveReferences();
            ApplyConfig();
        }

        private void OnEnable()
        {
            BeginGame();
        }

        private void OnDisable()
        {
            _isReadyToStart = false;
            _isPlaying = false;
            StopActors();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (_isReadyToStart && (CanAutoStart() || IsStartPressedThisFrame()))
            {
                StartGame();
            }
        }

        public void BeginGame()
        {
            ResolveReferences();
            ApplyConfig();

            _isReadyToStart = true;
            _isPlaying = false;

            player?.ResetState();

            if (ghosts != null)
            {
                int spawnCount = CacheGhostSceneSpawnPositions();
                ShuffleGhostSpawnPositions(spawnCount);

                for (int i = 0; i < ghosts.Length; i++)
                {
                    if (spawnCount > 0)
                    {
                        ghosts[i]?.ResetState(_shuffledGhostSpawnPositions[i % spawnCount]);
                    }
                    else
                    {
                        ghosts[i]?.ResetState();
                    }
                }
            }

            itemSpawner?.RespawnItems();
            StopActors();
        }

        public void StartGame()
        {
            if (!_isReadyToStart || _isPlaying)
            {
                return;
            }

            _isReadyToStart = false;
            _isPlaying = true;

            if (player != null)
            {
                player.ResetState();
            }
        }

        private void StopActors()
        {
            player?.StopMovement();
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                player = GetComponentInChildren<PacmanPlayerController>(true);
            }

            if (ghosts == null || ghosts.Length == 0)
            {
                ghosts = GetComponentsInChildren<PacmanGhostController>(true);
            }

            if (itemSpawner == null)
            {
                itemSpawner = GetComponentInChildren<PacmanItemSpawner>(true);
            }
        }

        private void ApplyConfig()
        {
            if (config == null)
            {
                return;
            }

            player?.Configure(config);

            if (ghosts != null)
            {
                for (int i = 0; i < ghosts.Length; i++)
                {
                    ghosts[i]?.Configure(config);
                }
            }

            itemSpawner?.Configure(config);
        }

        private int CacheGhostSceneSpawnPositions()
        {
            if (ghosts == null || ghosts.Length == 0)
            {
                return 0;
            }

            if (_ghostSpawnPositions.Length < ghosts.Length)
            {
                _ghostSpawnPositions = new Vector3[ghosts.Length];
                _shuffledGhostSpawnPositions = new Vector3[ghosts.Length];
            }

            int spawnCount = 0;
            for (int i = 0; i < ghosts.Length; i++)
            {
                PacmanGhostController ghost = ghosts[i];
                if (ghost == null)
                {
                    continue;
                }

                _ghostSpawnPositions[spawnCount] = ghost.GetInitialWorldPosition();
                spawnCount++;
            }

            return spawnCount;
        }

        private void ShuffleGhostSpawnPositions(int spawnCount)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                _shuffledGhostSpawnPositions[i] = _ghostSpawnPositions[i];
            }

            for (int i = spawnCount - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Vector3 temp = _shuffledGhostSpawnPositions[i];
                _shuffledGhostSpawnPositions[i] = _shuffledGhostSpawnPositions[randomIndex];
                _shuffledGhostSpawnPositions[randomIndex] = temp;
            }
        }

        private static bool IsStartPressedThisFrame()
        {
            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused ||
                 GameSceneManager.Instance.IsGameOver ||
                 GameSceneManager.Instance.IsTransitioning ||
                 GameSceneManager.Instance.IsInputGateActive))
            {
                return false;
            }

            // 일시정지 버튼 등 UI가 화면을 덮고 있는 동안의 탭은 게임 시작 신호로 보지 않는다
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        }

        private static bool CanAutoStart()
        {
            return GameSceneManager.Instance != null &&
                   GameSceneManager.Instance.CurrentGameId == 1 &&
                   !GameSceneManager.Instance.IsPaused &&
                   !GameSceneManager.Instance.IsGameOver &&
                   !GameSceneManager.Instance.IsTransitioning &&
                   !GameSceneManager.Instance.IsInputGateActive;
        }
    }
}
