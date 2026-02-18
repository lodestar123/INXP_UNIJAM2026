using FlappyBird.Game;
using FlappyBird.Interfaces.Game;
using UnityEngine;

namespace FlappyBird
{
    /// <summary>
    /// This script is attached to a trigger collider placed between the pipes.
    /// When the player enters this zone, it tells the GameManager to increment the score.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ScoreZone : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour gameFlowSource;

        private IFlappyBirdGameFlow _gameFlow;

        private void Awake()
        {
            // Best practice: Ensure the collider is set as a trigger in code,
            // reducing dependency on manual Unity Editor settings.
            var triggerCollider = GetComponent<BoxCollider2D>();
            if (!triggerCollider.isTrigger)
            {
                Debug.LogWarning($"The collider on {gameObject.name} was not set as a trigger. Forcing it now.", this);
                triggerCollider.isTrigger = true;
            }

            _gameFlow = ResolveGameFlow();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // It's a common and efficient practice to identify the player by a tag.
            // Make sure your player GameObject is tagged as "Player" in the Unity Editor.
            if (other.CompareTag("Player"))
            {
                _gameFlow ??= ResolveGameFlow();
                _gameFlow?.IncrementScore();
            }
        }

        private IFlappyBirdGameFlow ResolveGameFlow()
        {
            if (gameFlowSource is IFlappyBirdGameFlow typed)
            {
                return typed;
            }

            return FlappyBirdGameManager.Instance;
        }
    }
}
