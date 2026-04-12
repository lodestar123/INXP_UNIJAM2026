using UnityEngine;
using Utils;
using DG.Tweening;

namespace FallingDodge
{
    [RequireComponent(typeof(Collider2D))]
    public class FallingDodgeFallingObject : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private float despawnY = -7.5f;

        private FallingDodgeSpawner _spawner;
        private FallingDodgeGameManager _owner;
        private GameObject _sourcePrefab;
        private Item _item;
        private bool _isHazard;
        private float _fallSpeed;
        private int _groundSortingOrder;
        private string _groundSortingLayerName;
        private bool _isDespawning;
        private Sequence _effectSequence;

        public void Initialize(
            FallingDodgeSpawner spawner,
            FallingDodgeGameManager owner,
            GameObject sourcePrefab,
            bool isHazard,
            Item item,
            Sprite sprite,
            float fallSpeed,
            SpriteRenderer groundReference)
        {
            _spawner = spawner;
            _owner = owner;
            _sourcePrefab = sourcePrefab;
            _isHazard = isHazard;
            _item = item;
            _fallSpeed = fallSpeed;
            _isDespawning = false;

            _effectSequence?.Kill();
            _effectSequence = null;
            transform.DOKill();

            if (visual == null)
            {
                visual = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (visual != null)
            {
                visual.DOKill();
                visual.sprite = sprite;

                if (groundReference != null)
                {
                    _groundSortingOrder = groundReference.sortingOrder;
                    _groundSortingLayerName = groundReference.sortingLayerName;
                    visual.sortingLayerName = groundReference.sortingLayerName;
                    visual.sortingOrder = groundReference.sortingOrder + 1;
                }
                else
                {
                    _groundSortingOrder = visual.sortingOrder;
                    _groundSortingLayerName = visual.sortingLayerName;
                }

                Color color = visual.color;
                color.a = 1f;
                visual.color = color;
            }

            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.enabled = true;
            }
        }

        private void Update()
        {
            if (GameSceneManager.Instance == null || GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver)
            {
                return;
            }

            transform.position += Vector3.down * (_fallSpeed * Time.deltaTime);

            if (transform.position.y <= despawnY)
            {
                Despawn();
            }
        }

        private void OnDisable()
        {
            _effectSequence?.Kill();
            _effectSequence = null;
            transform.DOKill();

            if (visual != null)
            {
                visual.DOKill();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_owner == null || !other.CompareTag("Player"))
            {
                return;
            }

            if (_isHazard)
            {
                _owner.HandleHazardHit();
                Despawn();
            }
            else if (_item != null)
            {
                _owner.HandleItemCollected(_item);
                AnimateCollectAndDespawn();
            }
        }

        private void Despawn()
        {
            if (_isDespawning)
            {
                return;
            }

            _isDespawning = true;
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _effectSequence?.Kill();
            _effectSequence = null;
            transform.DOKill();

            if (visual != null)
            {
                visual.DOKill();
            }

            _spawner?.UnregisterSpawnedObject(gameObject);

            if (_sourcePrefab == null)
            {
                Destroy(gameObject);
                return;
            }

            ObjectPool.Instance.Return(_sourcePrefab, gameObject);
        }

        private void AnimateCollectAndDespawn()
        {
            if (_isDespawning)
            {
                return;
            }

            _isDespawning = true;
            _spawner?.UnregisterSpawnedObject(gameObject);

            Collider2D trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.enabled = false;
            }

            if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
            {
                GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.GetItem);
            }

            _effectSequence?.Kill();
            _effectSequence = DOTween.Sequence();
            _effectSequence.Append(transform.DOLocalMoveY(-16.0f, 0.6f).SetRelative().SetEase(Ease.OutQuad));

            if (visual != null)
            {
                _effectSequence.Join(visual.DOFade(0f, 0.5f).SetDelay(0.2f));
            }

            _effectSequence.OnComplete(ReturnToPool);
        }
    }
}
