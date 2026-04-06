using UnityEngine;
using Utils;

namespace FallingDodge
{
    public class FallingDodgeSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FallingDodgeGameManager gameManager;
        [SerializeField] private GameObject fallingObjectPrefab;
        [SerializeField] private SpriteRenderer groundReference;
        [SerializeField] private Sprite poopSprite;

        [Header("Spawn Area")]
        [SerializeField] private float spawnMinX = -2.8f;
        [SerializeField] private float spawnMaxX = 2.8f;
        [SerializeField] private float spawnY = 6.2f;

        [Header("Fall Speed")]
        [SerializeField] private float itemFallSpeedMin = 3.2f;
        [SerializeField] private float itemFallSpeedMax = 4.2f;
        [SerializeField] private float poopFallSpeedMin = 4.0f;
        [SerializeField] private float poopFallSpeedMax = 5.2f;

        [Header("Spawn Timing")]
        [SerializeField] private float baseSpawnInterval = 0.7f;
        [SerializeField] private float minimumSpawnInterval = 0.22f;

        [Header("Hazard Scaling")]
        [SerializeField] private float poopChanceAtStart = 0.12f;
        [SerializeField] private float poopChanceAtMax = 0.65f;
        [SerializeField] private float poopRampDuration = 70f;

        [Header("Wave Settings")]
        [SerializeField] private int initialPoolSize = 20;
        [SerializeField] private int minSpawnCountPerWave = 1;
        [SerializeField] private int maxSpawnCountPerWave = 2;

        [Header("Spawn Ranges")]
        [SerializeField] private int spawnRangeCount = 5;
        [SerializeField] private float spawnRangePadding = 0.15f;

        private float _elapsed;
        private float _spawnTimer;
        private bool _isRunning;

        private void OnDrawGizmosSelected()
        {
            int rangeCount = Mathf.Max(1, spawnRangeCount);
            float minX = Mathf.Min(spawnMinX, spawnMaxX);
            float maxX = Mathf.Max(spawnMinX, spawnMaxX);
            float totalWidth = maxX - minX;
            float gizmoHeight = 0.5f;

            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.9f);
            Gizmos.DrawLine(new Vector3(minX, spawnY, 0f), new Vector3(maxX, spawnY, 0f));
            Gizmos.DrawSphere(new Vector3(minX, spawnY, 0f), 0.08f);
            Gizmos.DrawSphere(new Vector3(maxX, spawnY, 0f), 0.08f);

            if (rangeCount == 1 || totalWidth <= 0f)
            {
                float width = Mathf.Max(0.1f, totalWidth);
                Vector3 leftTop = new Vector3(((minX + maxX) * 0.5f) - width * 0.5f, spawnY + gizmoHeight * 0.5f, 0f);
                Vector3 rightTop = new Vector3(((minX + maxX) * 0.5f) + width * 0.5f, spawnY + gizmoHeight * 0.5f, 0f);
                Vector3 leftBottom = new Vector3(leftTop.x, spawnY - gizmoHeight * 0.5f, 0f);
                Vector3 rightBottom = new Vector3(rightTop.x, spawnY - gizmoHeight * 0.5f, 0f);
                Gizmos.DrawLine(leftTop, rightTop);
                Gizmos.DrawLine(leftBottom, rightBottom);
                Gizmos.DrawLine(leftTop, leftBottom);
                Gizmos.DrawLine(rightTop, rightBottom);
                return;
            }

            float rangeWidth = totalWidth / rangeCount;
            float padding = Mathf.Clamp(spawnRangePadding, 0f, rangeWidth * 0.45f);

            for (int i = 0; i < rangeCount; i++)
            {
                float rawMin = minX + (rangeWidth * i);
                float rawMax = rawMin + rangeWidth;
                float paddedMin = rawMin + padding;
                float paddedMax = rawMax - padding;
                float centerX = (paddedMin + paddedMax) * 0.5f;
                float width = Mathf.Max(0.1f, paddedMax - paddedMin);

                Gizmos.color = (i % 2 == 0)
                    ? new Color(0.2f, 0.9f, 1f, 0.55f)
                    : new Color(0.1f, 0.6f, 1f, 0.55f);

                float left = centerX - width * 0.5f;
                float right = centerX + width * 0.5f;
                float top = spawnY + gizmoHeight * 0.5f;
                float bottom = spawnY - gizmoHeight * 0.5f;

                Gizmos.DrawLine(new Vector3(left, top, 0f), new Vector3(right, top, 0f));
                Gizmos.DrawLine(new Vector3(left, bottom, 0f), new Vector3(right, bottom, 0f));
                Gizmos.DrawLine(new Vector3(left, top, 0f), new Vector3(left, bottom, 0f));
                Gizmos.DrawLine(new Vector3(right, top, 0f), new Vector3(right, bottom, 0f));
            }
        }

        private void Awake()
        {
            if (fallingObjectPrefab != null)
            {
                ObjectPool.Instance.CreatePool(fallingObjectPrefab, initialPoolSize);
            }
        }

        public void ResetState()
        {
            _elapsed = 0f;
            _spawnTimer = 0f;
            _isRunning = false;
        }

        public void StartSpawning()
        {
            _elapsed = 0f;
            _spawnTimer = 0f;
            _isRunning = true;
        }

        public void StopSpawning()
        {
            _isRunning = false;
        }

        private void Update()
        {
            if (!_isRunning || GameSceneManager.Instance == null || GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            _spawnTimer += Time.deltaTime;

            float interval = Mathf.Lerp(baseSpawnInterval, minimumSpawnInterval, Mathf.Clamp01(_elapsed / poopRampDuration));
            if (_spawnTimer < interval)
            {
                return;
            }

            _spawnTimer = 0f;
            SpawnWave();
        }

        private void SpawnWave()
        {
            int minCount = Mathf.Max(1, minSpawnCountPerWave);
            int maxCount = Mathf.Max(minCount, maxSpawnCountPerWave);
            int spawnCount = Random.Range(minCount, maxCount + 1);

            if (spawnCount <= 1)
            {
                SpawnOne(Random.Range(spawnMinX, spawnMaxX));
                return;
            }

            int rangeCount = Mathf.Max(1, spawnRangeCount);
            spawnCount = Mathf.Min(spawnCount, rangeCount);

            int[] rangeIndices = new int[rangeCount];
            for (int i = 0; i < rangeCount; i++)
            {
                rangeIndices[i] = i;
            }

            for (int i = 0; i < rangeCount; i++)
            {
                int swapIndex = Random.Range(i, rangeCount);
                (rangeIndices[i], rangeIndices[swapIndex]) = (rangeIndices[swapIndex], rangeIndices[i]);
            }

            for (int i = 0; i < spawnCount; i++)
            {
                float spawnX = GetSpawnXInRange(rangeIndices[i], rangeCount);
                SpawnOne(spawnX);
            }
        }

        private float GetSpawnXInRange(int rangeIndex, int rangeCount)
        {
            float totalWidth = spawnMaxX - spawnMinX;
            if (totalWidth <= 0f || rangeCount <= 1)
            {
                return Random.Range(spawnMinX, spawnMaxX);
            }

            float rangeWidth = totalWidth / rangeCount;
            float rawMin = spawnMinX + (rangeWidth * rangeIndex);
            float rawMax = rawMin + rangeWidth;
            float padding = Mathf.Clamp(spawnRangePadding, 0f, rangeWidth * 0.45f);
            float paddedMin = rawMin + padding;
            float paddedMax = rawMax - padding;

            if (paddedMax <= paddedMin)
            {
                return (rawMin + rawMax) * 0.5f;
            }

            return Random.Range(paddedMin, paddedMax);
        }

        private void SpawnOne(float spawnX)
        {
            if (fallingObjectPrefab == null || gameManager == null)
            {
                return;
            }

            bool isHazard = Random.value < Mathf.Lerp(poopChanceAtStart, poopChanceAtMax, Mathf.Clamp01(_elapsed / poopRampDuration));
            Item item = null;
            Sprite sprite = poopSprite;
            float speed = Random.Range(Mathf.Min(poopFallSpeedMin, poopFallSpeedMax), Mathf.Max(poopFallSpeedMin, poopFallSpeedMax));

            if (!isHazard)
            {
                Item[] items = ItemDataBase.Items;
                if (items == null || items.Length == 0)
                {
                    return;
                }

                item = items[Random.Range(0, items.Length)];
                sprite = item != null ? item.sprite_Flappy : null;
                speed = Random.Range(Mathf.Min(itemFallSpeedMin, itemFallSpeedMax), Mathf.Max(itemFallSpeedMin, itemFallSpeedMax));
            }

            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0f);
            GameObject instance = ObjectPool.Instance.Spawn(fallingObjectPrefab, spawnPosition, Quaternion.identity);

            FallingDodgeFallingObject fallingObject = instance.GetComponent<FallingDodgeFallingObject>();
            if (fallingObject == null)
            {
                fallingObject = instance.AddComponent<FallingDodgeFallingObject>();
            }

            fallingObject.Initialize(gameManager, fallingObjectPrefab, isHazard, item, sprite, speed, groundReference);
        }
    }
}
