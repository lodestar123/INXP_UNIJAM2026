using System.Collections.Generic;
using UnityEngine;

namespace Pacman
{
    public class PacmanItemSpawner : MonoBehaviour
    {
        [Header("Item")]
        [SerializeField] private PacmanCollectibleItem itemPrefab;
        [SerializeField] private int itemCount = 49;
        [SerializeField] private float itemScale = 0.3f;
        [SerializeField] private bool spawnOnEnable = true;

        [Header("Spawn Points")]
        [SerializeField] private Transform spawnPointRoot;
        [SerializeField] private bool collectSpawnPointsFromRoot = true;
        [SerializeField] private bool createDefaultSpawnPointsIfMissing = true;
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

        private readonly List<PacmanCollectibleItem> _spawnedItems = new List<PacmanCollectibleItem>();
        private static readonly Vector2[] DefaultSpawnPositions =
        {
            new Vector2(-6.32f, 3.50f), new Vector2(-5.18f, 3.50f), new Vector2(-4.05f, 3.50f), new Vector2(-2.92f, 3.50f), new Vector2(-1.78f, 3.50f), new Vector2(1.78f, 3.50f), new Vector2(6.48f, 3.50f),
            new Vector2(-6.32f, 2.45f), new Vector2(-4.05f, 2.45f), new Vector2(-2.92f, 2.45f), new Vector2(-0.81f, 2.45f), new Vector2(0.81f, 2.45f), new Vector2(3.08f, 2.45f), new Vector2(6.48f, 2.45f),
            new Vector2(-6.32f, 1.22f), new Vector2(-4.05f, 1.22f), new Vector2(-1.78f, 1.22f), new Vector2(-0.81f, 1.22f), new Vector2(0.81f, 1.22f), new Vector2(4.21f, 1.22f), new Vector2(6.48f, 1.22f),
            new Vector2(-6.32f, 0.09f), new Vector2(-5.18f, 0.09f), new Vector2(-2.92f, 0.09f), new Vector2(-0.81f, 0.09f), new Vector2(0.81f, 0.09f), new Vector2(5.18f, 0.09f), new Vector2(6.48f, 0.09f),
            new Vector2(-6.32f, -1.14f), new Vector2(-4.05f, -1.14f), new Vector2(-1.78f, -1.14f), new Vector2(-0.81f, -1.14f), new Vector2(0.81f, -1.14f), new Vector2(4.21f, -1.14f), new Vector2(6.48f, -1.14f),
            new Vector2(-6.32f, -2.36f), new Vector2(-5.18f, -2.36f), new Vector2(-2.92f, -2.36f), new Vector2(-0.81f, -2.36f), new Vector2(0.81f, -2.36f), new Vector2(3.08f, -2.36f), new Vector2(6.48f, -2.36f),
            new Vector2(-6.32f, -3.50f), new Vector2(-4.05f, -3.50f), new Vector2(-1.78f, -3.50f), new Vector2(-0.81f, -3.50f), new Vector2(0.81f, -3.50f), new Vector2(4.21f, -3.50f), new Vector2(6.48f, -3.50f),
        };

        private void OnEnable()
        {
            if (spawnOnEnable)
            {
                RespawnItems();
            }
        }

        public void RespawnItems()
        {
            ClearSpawnedItems();
            RefreshSpawnPoints();

            if (spawnPoints.Count == 0 && createDefaultSpawnPointsIfMissing)
            {
                CreateDefaultSpawnPoints();
            }

            if (itemPrefab == null)
            {
                Debug.LogWarning("[PacmanItemSpawner] Item prefab is missing.", this);
                return;
            }

            if (spawnPoints.Count == 0)
            {
                Debug.LogWarning("[PacmanItemSpawner] Spawn points are missing.", this);
                return;
            }

            Item[] items = ItemDataBase.Items;
            if (items == null || items.Length == 0)
            {
                Debug.LogWarning("[PacmanItemSpawner] ItemDataBase has no items.", this);
                return;
            }

            int spawnCount = Mathf.Min(Mathf.Max(0, itemCount), spawnPoints.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                Transform spawnPoint = spawnPoints[i];
                if (spawnPoint == null)
                {
                    continue;
                }

                Item item = items[i % items.Length];
                PacmanCollectibleItem instance = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity, transform);
                instance.transform.localScale = Vector3.one * itemScale;
                instance.Initialize(item);
                _spawnedItems.Add(instance);
            }
        }

        public void ClearSpawnedItems()
        {
            for (int i = _spawnedItems.Count - 1; i >= 0; i--)
            {
                PacmanCollectibleItem item = _spawnedItems[i];
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            _spawnedItems.Clear();
        }

        [ContextMenu("Refresh Spawn Points")]
        private void RefreshSpawnPoints()
        {
            spawnPoints.RemoveAll(point => point == null);

            if (!collectSpawnPointsFromRoot || spawnPointRoot == null)
            {
                return;
            }

            spawnPoints.Clear();
            for (int i = 0; i < spawnPointRoot.childCount; i++)
            {
                Transform point = spawnPointRoot.GetChild(i);
                if (point != null && point.gameObject.activeSelf)
                {
                    spawnPoints.Add(point);
                }
            }
        }

        [ContextMenu("Create 49 Default Spawn Points")]
        private void CreateDefaultSpawnPoints()
        {
            if (spawnPointRoot == null)
            {
                GameObject root = new GameObject("PacmanItemSpawnPoints");
                root.transform.SetParent(transform, false);
                spawnPointRoot = root.transform;
            }

            for (int i = spawnPointRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = spawnPointRoot.GetChild(i);

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            int count = Mathf.Min(itemCount, DefaultSpawnPositions.Length);
            for (int i = 0; i < count; i++)
            {
                GameObject point = new GameObject($"SpawnPoint_{i:00}");
                point.transform.SetParent(spawnPointRoot, false);
                point.transform.localPosition = DefaultSpawnPositions[i];
            }

            RefreshSpawnPoints();
        }

        private void OnDrawGizmosSelected()
        {
            RefreshSpawnPoints();
            Gizmos.color = new Color(1f, 0.8f, 0.1f, 0.9f);

            int previewCount = Mathf.Min(Mathf.Max(0, itemCount), spawnPoints.Count);
            for (int i = 0; i < previewCount; i++)
            {
                Transform point = spawnPoints[i];
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.15f);
                }
            }
        }
    }
}
