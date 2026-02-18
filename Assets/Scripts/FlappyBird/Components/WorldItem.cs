using DG.Tweening;
using FlappyBird.Game;
using FlappyBird.Interfaces.Game;
using UnityEngine;

namespace FlappyBird.Components
{
    /// <summary>
    /// 월드에 배치된 아이템의 표시와 수집 처리를 담당합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldItem : MonoBehaviour, ICollectible
    {
        [SerializeField] private MonoBehaviour gameFlowSource;

        public Item ItemData { get; private set; }
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        private bool _isCollected = false;
        private IFlappyBirdGameFlow _gameFlow;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _gameFlow = ResolveGameFlow();
        }

        public void Initialize(Item item)
        {
            ItemData = item;
            
            ResetVisuals();
            
            if (_spriteRenderer is not null && item is not null)
            {
                _spriteRenderer.sprite = item.sprite_Flappy;
            }
        }
        
        public void AnimateCollect()
        {
            // 중복 수집 방지
            if (_isCollected) return;
            
            _isCollected = true;
            
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            Sequence effectSequence = DOTween.Sequence();

            effectSequence
                .Append(transform.DOLocalMoveY(-16.0f, 0.6f).SetRelative().SetEase(Ease.OutQuad)); // 아래로 낙하

            // 낙하와 함께 페이드아웃
            if (_spriteRenderer != null)
            {
                effectSequence.Join(_spriteRenderer.DOFade(0f, 0.5f).SetDelay(0.2f));
            }

            // 연출 종료 후 비활성화
            effectSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        public bool TryCollect(GameObject collector)
        {
            if (_isCollected)
            {
                return false;
            }

            _gameFlow ??= ResolveGameFlow();

            if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
            {
                GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.GetItem);
            }

            if (ItemData != null)
            {
                _gameFlow?.OnItemCollected(ItemData);
            }

            AnimateCollect();
            return true;
        }
        
        private void ResetVisuals()
        {
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            
            // 남아있는 트윈 정리
            transform.DOKill();
            if (_spriteRenderer != null)
            {
                _spriteRenderer.DOKill();
                
                // 알파값 복구
                Color color = _spriteRenderer.color;
                color.a = 1f;
                _spriteRenderer.color = color;
            }

            if (_collider != null)
            {
                _collider.enabled = true;
            }
            
            // 상태 초기화
            _isCollected = false;
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
