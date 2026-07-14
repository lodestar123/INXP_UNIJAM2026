using DG.Tweening;
using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적을 처치했을 때 아래로 떨어지는 아이템
    /// 플레이어가 닿으면 수집되어 애니팡 게임의 아이템 큐로 전달됨
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GalagaItemPickup : MonoBehaviour
    {
        private GalagaGameManager _owner;
        private Item _item;
        private float _fallSpeed;
        private float _despawnY;
        private bool _consumed;
        private SpriteRenderer _renderer;

        public void Initialize(GalagaGameManager owner, Item item, float fallSpeed, float despawnY, SpriteRenderer renderer)
        {
            _owner = owner;
            _item = item;
            _fallSpeed = fallSpeed;
            _despawnY = despawnY;
            _consumed = false;
            _renderer = renderer;

            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            col.enabled = true;
        }

        private void Update()
        {
            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            transform.position += Vector3.down * (_fallSpeed * Time.deltaTime);

            if (transform.position.y <= _despawnY)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;

            var player = other.GetComponent<GalagaPlayerController>();
            if (player == null || !player.IsAlive) return;

            _consumed = true;
            _owner?.HandleItemCollected(_item);

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // 수집 연출 후 제거
            transform.DOKill();
            transform.DOMoveY(transform.position.y + 0.6f, 0.25f).SetEase(Ease.OutQuad);
            if (_renderer != null)
            {
                _renderer.DOKill();
                _renderer.DOFade(0f, 0.25f).OnComplete(() => Destroy(gameObject));
            }
            else
            {
                Destroy(gameObject, 0.25f);
            }
        }

        private void OnDisable()
        {
            transform.DOKill();
            if (_renderer != null) _renderer.DOKill();
        }
    }
}
