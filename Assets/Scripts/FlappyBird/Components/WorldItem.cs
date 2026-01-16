using UnityEngine;

namespace FlappyBird.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldItem : MonoBehaviour
    {
        public Item ItemData { get; private set; }
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Item item)
        {
            ItemData = item;
            if (_spriteRenderer != null && item != null)
            {
                _spriteRenderer.sprite = item.sprite;
            }
        }
    }
}
