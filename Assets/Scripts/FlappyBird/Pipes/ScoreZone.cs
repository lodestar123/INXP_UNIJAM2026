using FlappyBird.Game;
using FlappyBird.Interfaces.Game;
using UnityEngine;

namespace FlappyBird
{
    /// <summary>
    /// 플레이어가 통과하면 점수를 1 증가시키는 트리거입니다.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ScoreZone : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour gameFlowSource;

        private IFlappyBirdGameFlow _gameFlow;

        private void Awake()
        {
            // 점수 구간은 항상 트리거로 동작해야 합니다.
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
            // Player 태그일 때만 점수 처리
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
