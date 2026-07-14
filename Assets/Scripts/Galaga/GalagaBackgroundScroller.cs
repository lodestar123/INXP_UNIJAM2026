using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 배경 스크롤러
    /// </summary>
    public class GalagaBackgroundScroller : MonoBehaviour
    {
        private float _scrollSpeed;
        private Transform _tileA;
        private Transform _tileB;
        private float _tileHeight;

        public void Initialize(float scrollSpeed, Sprite tileSprite, float worldHeight, int sortingOrder)
        {
            _scrollSpeed = scrollSpeed;

            _tileA = CreateTile("BackgroundTileA", tileSprite, worldHeight, sortingOrder, out _tileHeight);
            _tileB = CreateTile("BackgroundTileB", tileSprite, worldHeight, sortingOrder, out _);

            _tileA.localPosition = Vector3.zero;
            _tileB.localPosition = new Vector3(0f, _tileHeight, 0f);
        }

        private Transform CreateTile(string name, Sprite sprite, float worldHeight, int sortingOrder, out float tileHeight)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            float spriteHeight = sprite.bounds.size.y;
            float spriteWidth = sprite.bounds.size.x;
            float scaleY = worldHeight / Mathf.Max(0.01f, spriteHeight);
            float scale = scaleY;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            tileHeight = spriteHeight * scale;
            float scaledWidth = spriteWidth * scale;
            if (scaledWidth < worldHeight * 2f)
            {
                float wScale = (worldHeight * 2f) / Mathf.Max(0.01f, spriteWidth);
                go.transform.localScale = new Vector3(Mathf.Max(scale, wScale), scale, 1f);
            }

            return go.transform;
        }

        private void Update()
        {
            if (_tileA == null || _tileB == null) return;

            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            float delta = _scrollSpeed * Time.deltaTime;
            _tileA.localPosition += Vector3.down * delta;
            _tileB.localPosition += Vector3.down * delta;

            RecycleIfNeeded(_tileA);
            RecycleIfNeeded(_tileB);
        }

        private void RecycleIfNeeded(Transform tile)
        {
            if (tile.localPosition.y <= -_tileHeight)
            {
                Transform other = tile == _tileA ? _tileB : _tileA;
                tile.localPosition = new Vector3(0f, other.localPosition.y + _tileHeight, 0f);
            }
        }
    }
}
