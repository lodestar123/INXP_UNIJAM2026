using FlappyBird.Game;
using FlappyBird.Interfaces.Game;
using UnityEngine;
using Utils;

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
            BoxCollider2D triggerCollider = GetComponent<BoxCollider2D>();
            if (!triggerCollider.isTrigger) // 트리거로 설정되어 있지 않으면 강제로 트리거로 설정
            {
                CustomLog.Warn($"The collider on {gameObject.name} was not set as a trigger. Forcing it now.", this);
                triggerCollider.isTrigger = true;
            }

            _gameFlow = ResolveGameFlow(); // 게임 플로우 인터페이스 참조 시도
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Player 태그일 때만 점수 처리
            if (other.CompareTag("Player"))
            {
                _gameFlow ??= ResolveGameFlow(); // 게임 플로우 인터페이스가 아직 참조되지 않았다면 다시 시도
                _gameFlow?.IncrementScore(); // 게임 플로우 인터페이스가 유효할 때만 점수 증가 호출
            }
        }

        // 게임 플로우 인터페이스를 유연하게 참조하기 위한 헬퍼 메서드
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
