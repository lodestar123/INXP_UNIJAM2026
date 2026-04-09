using FlappyBird.Game;
using FlappyBird.Interfaces.Game;
using UnityEngine;

namespace FlappyBird.Player
{
    /// <summary>
    /// 충돌 시 게임 종료 및 아이템 트리거를 처리합니다.
    /// </summary>
    [RequireComponent(typeof(FlappyBirdPlayer))]
    public class FlappyBirdPlayerCollisionHandler : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour gameFlowSource;

        private FlappyBirdPlayer _player;
        private IFlappyBirdGameFlow _gameFlow;

        private void Awake()
        {
            _player = GetComponent<FlappyBirdPlayer>();
            _gameFlow = ResolveGameFlow();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_player.IsPlayerActive)
            {
                return;
            }

            _gameFlow = ResolveGameFlow();
            _gameFlow?.EndGame();
            _player.DeactivatePlayer();
            _player.PlayDeathAnimation(() =>
            {
                _gameFlow?.TransitionToNextGame();
            });
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_player.IsPlayerActive || !other.CompareTag("Item"))
            {
                return;
            }

            ICollectible collectible = other.GetComponent<ICollectible>();
            if (collectible != null)
            {
                collectible.TryCollect(gameObject);
                return;
            }

            other.gameObject.SetActive(false);
        }

        private IFlappyBirdGameFlow ResolveGameFlow()
        {
            if (gameFlowSource != null && gameFlowSource is IFlappyBirdGameFlow typed)
            {
                return typed;
            }

            return FlappyBirdGameManager.Instance;
        }
    }
}
