using FallingDodge.Configs;
using UnityEngine;
using Utils;
using System.Collections.Generic;

namespace FallingDodge
{
    public class FallingDodgeSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FallingDodgeGameManager gameManager;
        [SerializeField] private GameObject fallingObjectPrefab;
        [SerializeField] private SpriteRenderer groundReference;
        [SerializeField] private Sprite poopSprite;
        [SerializeField] private FallingDodgeConfig config;

        private float _elapsed;
        private float _spawnTimer;
        private bool _isRunning;
        private readonly HashSet<GameObject> _activeSpawnedObjects = new HashSet<GameObject>();
        private Item _lastCollectedItem;
        private Item _blockedCollectedItem;
        private int _consecutiveCollectedItemCount;

        public float CurrentFallSpeedMultiplier { get; private set; } = 1f;

        private void OnDrawGizmosSelected()
        {
            FallingDodgeConfig activeConfig = config;
            if (activeConfig == null)
            {
                return;
            }

            int rangeCount = Mathf.Max(1, activeConfig.SpawnRangeCount);
            float minX = Mathf.Min(activeConfig.SpawnMinX, activeConfig.SpawnMaxX);
            float maxX = Mathf.Max(activeConfig.SpawnMinX, activeConfig.SpawnMaxX);
            float totalWidth = maxX - minX;
            float gizmoHeight = 0.5f;

            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.9f);
            Gizmos.DrawLine(new Vector3(minX, activeConfig.SpawnY, 0f), new Vector3(maxX, activeConfig.SpawnY, 0f));
            Gizmos.DrawSphere(new Vector3(minX, activeConfig.SpawnY, 0f), 0.08f);
            Gizmos.DrawSphere(new Vector3(maxX, activeConfig.SpawnY, 0f), 0.08f);

            if (rangeCount == 1 || totalWidth <= 0f)
            {
                float width = Mathf.Max(0.1f, totalWidth);
                Vector3 leftTop = new Vector3(((minX + maxX) * 0.5f) - width * 0.5f, activeConfig.SpawnY + gizmoHeight * 0.5f, 0f);
                Vector3 rightTop = new Vector3(((minX + maxX) * 0.5f) + width * 0.5f, activeConfig.SpawnY + gizmoHeight * 0.5f, 0f);
                Vector3 leftBottom = new Vector3(leftTop.x, activeConfig.SpawnY - gizmoHeight * 0.5f, 0f);
                Vector3 rightBottom = new Vector3(rightTop.x, activeConfig.SpawnY - gizmoHeight * 0.5f, 0f);
                Gizmos.DrawLine(leftTop, rightTop);
                Gizmos.DrawLine(leftBottom, rightBottom);
                Gizmos.DrawLine(leftTop, leftBottom);
                Gizmos.DrawLine(rightTop, rightBottom);
                return;
            }

            float rangeWidth = totalWidth / rangeCount;
            float padding = Mathf.Clamp(activeConfig.SpawnRangePadding, 0f, rangeWidth * 0.45f);

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
                float top = activeConfig.SpawnY + gizmoHeight * 0.5f;
                float bottom = activeConfig.SpawnY - gizmoHeight * 0.5f;

                Gizmos.DrawLine(new Vector3(left, top, 0f), new Vector3(right, top, 0f));
                Gizmos.DrawLine(new Vector3(left, bottom, 0f), new Vector3(right, bottom, 0f));
                Gizmos.DrawLine(new Vector3(left, top, 0f), new Vector3(left, bottom, 0f));
                Gizmos.DrawLine(new Vector3(right, top, 0f), new Vector3(right, bottom, 0f));
            }
        }

        private void Awake()
        {
            if (fallingObjectPrefab != null && config != null)
            {
                ObjectPool.Instance.CreatePool(fallingObjectPrefab, config.InitialPoolSize);
            }
        }

        public void ResetState()
        {
            _elapsed = 0f;
            _spawnTimer = 0f;
            _isRunning = false;
            CurrentFallSpeedMultiplier = 1f;
            ResetCollectedItemLimit();
            DespawnAllSpawnedObjects();
        }

        public void StartSpawning()
        {
            _elapsed = 0f;
            _spawnTimer = config != null ? -Mathf.Max(0f, config.InitialSpawnDelay) : 0f;
            CurrentFallSpeedMultiplier = 1f;
            ResetCollectedItemLimit();
            _isRunning = true;
        }

        public void StopSpawning()
        {
            _isRunning = false;
            DespawnAllSpawnedObjects();
        }

        public void UnregisterSpawnedObject(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            _activeSpawnedObjects.Remove(instance);
        }

        public float GetScaledFallSpeed(float baseFallSpeed)
        {
            return baseFallSpeed * CurrentFallSpeedMultiplier;
        }

        public void NotifyItemCollected(Item item)
        {
            if (item == null)
            {
                return;
            }

            if (item == _lastCollectedItem)
            {
                _consecutiveCollectedItemCount++;
            }
            else
            {
                _lastCollectedItem = item;
                _consecutiveCollectedItemCount = 1;
                _blockedCollectedItem = null;
            }

            if (_consecutiveCollectedItemCount >= 3)
            {
                _blockedCollectedItem = item;
            }
        }

        private void Update()
        {
            if (!_isRunning || GameSceneManager.Instance == null || GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            _spawnTimer += Time.deltaTime;
            if (config == null)
            {
                return;
            }

            CurrentFallSpeedMultiplier = Mathf.Min(
                Mathf.Max(1f, config.MaxFallSpeedMultiplier),
                1f + Mathf.Max(0f, config.FallSpeedAcceleration) * _elapsed);

            float interval = Mathf.Lerp(config.BaseSpawnInterval, config.MinimumSpawnInterval, Mathf.Clamp01(_elapsed / config.PoopRampDuration));
            if (_spawnTimer < interval)
            {
                return;
            }

            _spawnTimer = 0f;
            SpawnWave();
        }

        private void SpawnWave()
        {
            int minCount = Mathf.Max(1, config.MinSpawnCountPerWave);
            int maxCount = Mathf.Max(minCount, config.MaxSpawnCountPerWave);
            int spawnCount = Random.Range(minCount, maxCount + 1);

            if (spawnCount <= 1)
            {
                SpawnOne(Random.Range(config.SpawnMinX, config.SpawnMaxX));
                return;
            }

            int rangeCount = Mathf.Max(1, config.SpawnRangeCount);
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
            float totalWidth = config.SpawnMaxX - config.SpawnMinX;
            if (totalWidth <= 0f || rangeCount <= 1)
            {
                return Random.Range(config.SpawnMinX, config.SpawnMaxX);
            }

            float rangeWidth = totalWidth / rangeCount;
            float rawMin = config.SpawnMinX + (rangeWidth * rangeIndex);
            float rawMax = rawMin + rangeWidth;
            float padding = Mathf.Clamp(config.SpawnRangePadding, 0f, rangeWidth * 0.45f);
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
            if (fallingObjectPrefab == null || gameManager == null || config == null)
            {
                return;
            }

            bool isHazard = Random.value < Mathf.Lerp(config.PoopChanceAtStart, config.PoopChanceAtMax, Mathf.Clamp01(_elapsed / config.PoopRampDuration));
            Item item = null;
            Sprite sprite = poopSprite;
            float speed = Random.Range(Mathf.Min(config.PoopFallSpeedMin, config.PoopFallSpeedMax), Mathf.Max(config.PoopFallSpeedMin, config.PoopFallSpeedMax));

            if (!isHazard)
            {
                Item[] items = ItemDataBase.Items;
                if (items == null || items.Length == 0)
                {
                    return;
                }

                item = SelectSpawnItem(items);
                if (item == null)
                {
                    return;
                }

                sprite = item != null ? item.sprite_Flappy : null;
                speed = Random.Range(Mathf.Min(config.ItemFallSpeedMin, config.ItemFallSpeedMax), Mathf.Max(config.ItemFallSpeedMin, config.ItemFallSpeedMax));
            }

            Vector3 spawnPosition = new Vector3(spawnX, config.SpawnY, 0f);
            GameObject instance = ObjectPool.Instance.Spawn(fallingObjectPrefab, spawnPosition, Quaternion.identity);
            _activeSpawnedObjects.Add(instance);

            FallingDodgeFallingObject fallingObject = instance.GetComponent<FallingDodgeFallingObject>();
            if (fallingObject == null)
            {
                fallingObject = instance.AddComponent<FallingDodgeFallingObject>();
            }

            fallingObject.Initialize(this, gameManager, fallingObjectPrefab, isHazard, item, sprite, speed, groundReference);
        }

        private Item SelectSpawnItem(Item[] items)
        {
            if (_blockedCollectedItem == null || items.Length <= 1)
            {
                return items[Random.Range(0, items.Length)];
            }

            int selectableCount = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && items[i] != _blockedCollectedItem)
                {
                    selectableCount++;
                }
            }

            if (selectableCount == 0)
            {
                return null;
            }

            int selectedIndex = Random.Range(0, selectableCount);
            for (int i = 0; i < items.Length; i++)
            {
                Item candidate = items[i];
                if (candidate == null || candidate == _blockedCollectedItem)
                {
                    continue;
                }

                if (selectedIndex == 0)
                {
                    return candidate;
                }

                selectedIndex--;
            }

            return null;
        }

        private void ResetCollectedItemLimit()
        {
            _lastCollectedItem = null;
            _blockedCollectedItem = null;
            _consecutiveCollectedItemCount = 0;
        }

        private void DespawnAllSpawnedObjects()
        {
            if (_activeSpawnedObjects.Count == 0 || fallingObjectPrefab == null || ObjectPool.Instance == null)
            {
                _activeSpawnedObjects.Clear();
                return;
            }

            GameObject[] activeObjects = new GameObject[_activeSpawnedObjects.Count];
            _activeSpawnedObjects.CopyTo(activeObjects);
            _activeSpawnedObjects.Clear();

            for (int i = 0; i < activeObjects.Length; i++)
            {
                GameObject instance = activeObjects[i];
                if (instance == null)
                {
                    continue;
                }

                ObjectPool.Instance.Return(fallingObjectPrefab, instance);
            }
        }
    }
}
