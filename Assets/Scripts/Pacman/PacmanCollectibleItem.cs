using DG.Tweening;
using UnityEngine;

namespace Pacman
{
    /// <summary>
    /// 팩맨 맵 위 수집 아이템.
    /// 플레이어가 먹으면 Anipang 아이템 큐로 전달함.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PacmanCollectibleItem : MonoBehaviour
    {
        [SerializeField] private Item itemData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        // 먹었을 때 아래로 빨려 들어가는 연출 거리/시간.
        [SerializeField] private float collectMoveDistance = 0.8f;
        [SerializeField] private float collectDuration = 0.15f;

        private Collider2D _collider2D;
        private bool _isCollected;

        /// <summary>
        /// Spawner가 아이템 데이터를 주입할 때 호출함.
        /// </summary>
        public void Initialize(Item item)
        {
            itemData = item;
            ApplySprite();
            _isCollected = false;

            if (_collider2D == null)
            {
                _collider2D = GetComponent<Collider2D>();
            }

            _collider2D.isTrigger = true;
            _collider2D.enabled = true;
        }

        private void Awake()
        {
            _collider2D = GetComponent<Collider2D>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            ApplySprite();
        }

        private void OnEnable()
        {
            _isCollected = false;

            if (_collider2D != null)
            {
                _collider2D.isTrigger = true;
                _collider2D.enabled = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected || IsGameStopped())
            {
                return;
            }

            if (!other.TryGetComponent<PacmanPlayerController>(out _) && !other.CompareTag("Player"))
            {
                return;
            }

            Collect();
        }

        /// <summary>
        /// 중복 수집 방지 후 아이템 큐에 전달하고 비활성화함.
        /// </summary>
        private void Collect()
        {
            if (itemData == null)
            {
                Debug.LogWarning("[PacmanCollectibleItem] ItemData is missing.", this);
                return;
            }

            _isCollected = true;
            _collider2D.enabled = false;
            FlappyItemCollector.CollectItem(itemData);

            transform.DOKill();
            transform
                .DOMove(transform.position + Vector3.down * collectMoveDistance, collectDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void ApplySprite()
        {
            if (spriteRenderer == null || itemData == null)
            {
                return;
            }

            // Stage1/2 아이템 비주얼 우선 사용, 없으면 Anipang 스프라이트 사용함.
            spriteRenderer.sprite = itemData.sprite_Flappy != null
                ? itemData.sprite_Flappy
                : itemData.sprite_AniPang;
        }

        private static bool IsGameStopped()
        {
            return GameSceneManager.Instance != null &&
                   (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver);
        }
    }
}
