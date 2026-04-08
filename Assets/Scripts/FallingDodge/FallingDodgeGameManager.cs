using UnityEngine;
using DG.Tweening;
using FlappyBird.Player;

namespace FallingDodge
{
    public class FallingDodgeGameManager : MonoBehaviour
    {
        [SerializeField] private FallingDodgePlayerController player;
        [SerializeField] private FallingDodgeSpawner spawner;
        [SerializeField] private int scorePerItem = 100;

        private bool _isEnding;
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

        private void BeginGame()
        {
            _isEnding = false;
            EnsureDeathAnimator();
            _deathAnimator?.Cancel();
            player?.ResetState();
            spawner?.StartSpawning();
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
            if (player != null)
            {
                player.ResetState();
            }

            if (spawner != null)
            {
                spawner.ResetState();
            }
        }

        public void OnEnterGame()
        {
            if (player != null)
            {
                player.ResetState();
            }

            spawner?.StartSpawning();
        }

        public void OnExitGame()
        {
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
            // GameSceneManager.Instance.AddScore(scorePerItem);
        }

        public void HandleHazardHit()
        {
            if (_isEnding)
            {
                return;
            }

            _isEnding = true;
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
