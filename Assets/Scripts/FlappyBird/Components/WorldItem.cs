using DG.Tweening;
using UnityEngine;

namespace FlappyBird.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldItem : MonoBehaviour
    {
        public Item ItemData { get; private set; }
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
            // 1. 중복 획득 방지를 위해 콜라이더 비활성화
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            // 2. DOTween 시퀀스 생성 (var 미사용)
            Sequence effectSequence = DOTween.Sequence();

            // 효과: 위로 살짝 커지면서 솟았다가(Jump), 아래로 툭 떨어짐(Fall)
            // Join을 사용하여 크기 변화와 이동을 동시에 수행
            effectSequence
                // .Append(transform.DOScale(Vector3.one * 1.4f, 0.1f)) // 0.1초 동안 커짐
                .Join(transform.DOLocalMoveY(0.5f, 0.15f).SetRelative().SetEase(Ease.OutQuad)) // 위로 살짝
                .Append(transform.DOLocalMoveY(-8.0f, 0.6f).SetRelative().SetEase(Ease.OutBounce)); // 아래로 튕기며 추락

            // 투명해지는 효과 동시 진행 (스프라이트가 있을 경우)
            if (_spriteRenderer != null)
            {
                effectSequence.Join(_spriteRenderer.DOFade(0f, 0.5f).SetDelay(0.2f));
            }

            // 3. 종료 후 비활성화
            effectSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }
        
        private void ResetVisuals()
        {
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            
            // DOTween 애니메이션 중단 (혹시 실행 중이라면)
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
        }
    }
}
