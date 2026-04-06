using UnityEngine;

namespace FallingDodge
{
    public class FallingDodgeGameManager : MonoBehaviour
    {
        [SerializeField] private FallingDodgePlayerController player;
        [SerializeField] private FallingDodgeSpawner spawner;
        [SerializeField] private int scorePerItem = 100;

        private void Start()
        {
            if (GameSceneManager.Instance != null && GameSceneManager.Instance.isActiveAndEnabled)
            {
                return;
            }

            player?.ResetState();
            spawner?.StartSpawning();
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
            GameSceneManager.Instance.AddScore(scorePerItem);
        }

        public void HandleHazardHit()
        {
            if (GameSceneManager.Instance == null)
            {
                return;
            }

            spawner?.StopSpawning();
            player?.StopMovement();
            Debug.LogWarning("[FallingDodgeGameManager] TriggerGameOver hook was reverted from GameSceneManager. FallingDodge hazard death is temporarily disabled.");
        }
    }
}
