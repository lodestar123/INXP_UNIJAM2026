using FlappyBird.Interfaces.Game;
using UnityEngine;

namespace FlappyBird.Game
{
    /// <summary>
    /// 플레이어가 닿으면 획득되는 기본 아이템 컴포넌트입니다.
    /// </summary>
    public class FlappyBirdItem : MonoBehaviour, ICollectible
    {
        [SerializeField] private MonoBehaviour gameFlowSource;

        [Tooltip("이 오브젝트가 담고 있는 아이템 데이터입니다.")]
        public Item itemData;
        
        private bool _isCollected = false; // 중복 수집 방지
        private IFlappyBirdGameFlow _gameFlow;
        
        private void Awake()
        {
            _gameFlow = ResolveGameFlow();

            if (TryGetComponent<FlappyBird.Components.WorldItem>(out _))
            {
                enabled = false;
                return;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 이미 수집됐거나 플레이어가 아니면 무시
            if (_isCollected || !other.CompareTag("Player")) return;
            
            // WorldItem이 있으면 해당 컴포넌트가 수집을 처리
            if (TryGetComponent<FlappyBird.Components.WorldItem>(out _))
            {
                return;
            }
            
            TryCollect(other.gameObject);
        }

        public bool TryCollect(GameObject collector)
        {
            if (_isCollected)
            {
                return false;
            }

            _gameFlow ??= ResolveGameFlow();

            _isCollected = true;

            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            _gameFlow?.OnItemCollected(itemData);
            gameObject.SetActive(false);
            return true;
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
