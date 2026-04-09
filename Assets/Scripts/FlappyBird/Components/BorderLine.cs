using FlappyBird.Game;
using FlappyBird.Interfaces.Game;
using UnityEngine;

namespace FlappyBird.Components
{
    /// <summary>
    /// 플레이어가 경계선에 닿으면 게임 종료를 요청합니다.
    /// </summary>
    public class BorderLine : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour gameFlowSource;

        private IFlappyBirdGameFlow _gameFlow;

        private void Awake()
        {
            _gameFlow = ResolveGameFlow();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _gameFlow = ResolveGameFlow();
                _gameFlow?.EndGame();
            }
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
