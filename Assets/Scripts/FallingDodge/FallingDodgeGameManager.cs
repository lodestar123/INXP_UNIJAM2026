using UnityEngine;
using DG.Tweening;
using FlappyBird.Player;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FallingDodge
{
    public class FallingDodgeGameManager : MonoBehaviour
    {
        [SerializeField] private FallingDodgePlayerController player;
        [SerializeField] private FallingDodgeSpawner spawner;
        [SerializeField] private int scorePerItem = 100;

        private bool _isEnding;
        private bool _isReadyToStart;
        private bool _isPlaying;
        private FlappyBirdPlayerDeathAnimator _deathAnimator;

        private void OnEnable()
        {
            BeginGame();
        }

        private void Start()
        {
            if (GameSceneManager.Instance != null && GameSceneManager.Instance.isActiveAndEnabled)
            {
                return;
            }

            BeginGame();
        }

        private void OnDisable()
        {
            spawner?.StopSpawning();
            player?.StopMovement();
        }

        private void Update()
        {
            if (player == null || spawner == null)
            {
                return;
            }

            player.SetMoveSpeedMultiplier(spawner.CurrentFallSpeedMultiplier);

            if (_isReadyToStart && IsStartPressedThisFrame())
            {
                StartGame();
            }
        }

        private void BeginGame()
        {
            _isEnding = false;
            _isReadyToStart = false;
            _isPlaying = false;
            EnsureDeathAnimator();
            _deathAnimator?.Cancel();
            spawner?.StopSpawning();
            if (player != null)
            {
                player.ResetState(onComplete: EnterReadyState);
            }
            else
            {
                EnterReadyState();
            }
        }   

        private void EnterReadyState()
        {
            if (_isEnding || (GameSceneManager.Instance != null && GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            player?.StopMovement();
            _isReadyToStart = true;
        }

        private void StartGame()
        {
            if (!_isReadyToStart || _isPlaying)
            {
                return;
            }

            _isReadyToStart = false;
            _isPlaying = true;
            player?.StartMovement();
            spawner?.StartSpawning();
        }

        private bool IsStartPressedThisFrame()
        {
            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver ||
                 GameSceneManager.Instance.IsTransitioning || GameSceneManager.Instance.IsInputGateActive))
            {
                return false;
            }

            // StartAalarm 같은 UI가 화면을 덮고 있는 동안의 탭은 게임 시작 신호로 보지 않는다
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        }

        private void EnsureDeathAnimator()
        {
            if (player == null)
            {
                return;
            }

            if (_deathAnimator == null)
            {
                _deathAnimator = player.GetComponent<FlappyBirdPlayerDeathAnimator>();
            }

            if (_deathAnimator == null)
            {
                _deathAnimator = player.gameObject.AddComponent<FlappyBirdPlayerDeathAnimator>();
            }
        }

        public void ResetState()
        {
            _isReadyToStart = false;
            _isPlaying = false;

            if (player != null)
            {
                player.ResetState(onComplete: EnterReadyState);
            }

            if (spawner != null)
            {
                spawner.ResetState();
            }
        }

        public void OnEnterGame()
        {
            BeginGame();
        }

        public void OnExitGame()
        {
            _isReadyToStart = false;
            _isPlaying = false;
            spawner?.StopSpawning();
            player?.StopMovement();
        }

        public void HandleItemCollected(Item item)
        {
            if (GameSceneManager.Instance == null || item == null)
            {
                return;
            }

            FlappyItemCollector.CollectItem(item);
            spawner?.NotifyItemCollected(item);
            // GameSceneManager.Instance.AddScore(scorePerItem);
        }

        public void HandleHazardHit()
        {
            if (_isEnding)
            {
                return;
            }

            _isEnding = true;
            _isReadyToStart = false;
            _isPlaying = false;
            spawner?.StopSpawning();
            player?.StopMovement();

            if (GameSceneManager.Instance == null)
            {
                Debug.LogWarning("[FallingDodgeGameManager] GameSceneManager가 없어 Present 전환을 실행할 수 없습니다.");
                return;
            }

            EnsureDeathAnimator();

            TweenCallback complete = () =>
            {
                if (GameSceneManager.Instance != null && GameSceneManager.Instance.CurrentGameId == 1)
                {
                    GameSceneManager.Instance.OnChangeGame();
                }
            };

            if (_deathAnimator != null && player != null)
            {
                _deathAnimator.Play(complete);
                return;
            }

            complete();
        }
    }
}
