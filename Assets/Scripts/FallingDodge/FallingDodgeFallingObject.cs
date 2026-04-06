using UnityEngine;
using Utils;

namespace FallingDodge
{
    [RequireComponent(typeof(Collider2D))]
    public class FallingDodgeFallingObject : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private float despawnY = -7.5f;

        private FallingDodgeGameManager _owner;
        private GameObject _sourcePrefab;
        private Item _item;
        private bool _isHazard;
        private float _fallSpeed;
        private float _groundY;
        private int _groundSortingOrder;
        private string _groundSortingLayerName;
        private bool _didGoBehindGround;

        public void Initialize(
            FallingDodgeGameManager owner,
            GameObject sourcePrefab,
            bool isHazard,
            Item item,
            Sprite sprite,
            float fallSpeed,
            SpriteRenderer groundReference)
        {
            _owner = owner;
            _sourcePrefab = sourcePrefab;
            _isHazard = isHazard;
            _item = item;
            _fallSpeed = fallSpeed;
            _didGoBehindGround = false;

            if (visual == null)
            {
                visual = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (visual != null)
            {
                visual.sprite = sprite;

                if (groundReference != null)
                {
                    _groundY = groundReference.bounds.max.y;
                    _groundSortingOrder = groundReference.sortingOrder;
                    _groundSortingLayerName = groundReference.sortingLayerName;
                    visual.sortingLayerName = groundReference.sortingLayerName;
                    visual.sortingOrder = groundReference.sortingOrder + 1;
                }
                else
                {
                    _groundY = despawnY;
                    _groundSortingOrder = visual.sortingOrder;
                    _groundSortingLayerName = visual.sortingLayerName;
                }
            }
        }

        private void Update()
        {
            if (GameSceneManager.Instance == null || GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver)
            {
                return;
            }

            transform.position += Vector3.down * (_fallSpeed * Time.deltaTime);

            if (!_didGoBehindGround && visual != null && transform.position.y <= _groundY)
            {
                _didGoBehindGround = true;
                visual.sortingLayerName = _groundSortingLayerName;
                visual.sortingOrder = _groundSortingOrder - 1;
            }

            if (transform.position.y <= despawnY)
            {
                Despawn();
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
            }
            else if (_item != null)
            {
                _owner.HandleItemCollected(_item);
            }

            Despawn();
        }

        private void Despawn()
        {
            if (_sourcePrefab == null)
            {
                Destroy(gameObject);
                return;
            }

            ObjectPool.Instance.Return(_sourcePrefab, gameObject);
        }
    }
}
