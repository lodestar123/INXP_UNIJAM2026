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
            if (_isReadyToStart && IsStartPressedThisFrame())
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
                for (int i = 0; i < ghosts.Length; i++)
                {
                    ghosts[i]?.ResetState();
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
    }
}
