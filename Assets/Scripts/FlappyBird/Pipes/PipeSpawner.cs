using System.Collections.Generic;
using FlappyBird.Components;
using FlappyBird.Configs;
using FlappyBird.Interfaces.Pipes;
using UnityEngine;
using Utils;

namespace FlappyBird
{
    /// <summary>
    /// 파이프/아이템을 생성하고 스크롤 이동을 제어합니다.
    /// </summary>
    public class PipeSpawner : MonoBehaviour, IScrollSpeedProvider
    {
        [Header("설정")]
        [SerializeField] private FlappyBirdConfig config;

        public float CurrentScrollSpeed { get; private set; }
        public bool IsScrolling { get; private set; }

        private readonly DistanceSpawnScheduler _spawnScheduler = new DistanceSpawnScheduler();
        private readonly SpawnedMoverRegistry _moverRegistry = new SpawnedMoverRegistry();
        private readonly IPipePatternGenerator _patternGenerator = new DefaultPipePatternGenerator();

        private bool _isSpawning;
        private float _spawnIntervalDistance;

        private float? _prevItemY;
        private Item _lastSpawnedItem;
        private int _consecutiveItemCount;

        private const string TAG_PIPE = "Pipe";
        private const string TAG_ITEM = "Item";

        private void Awake()
        {
            IsScrolling = false;
            if (config != null)
            {
                CurrentScrollSpeed = config.PipeMoveSpeed;
                _spawnIntervalDistance = config.PipeMoveSpeed * config.PipeSpawnInterval;
            }
        }

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError("PipeSpawner: 설정 파일이 누락되었습니다.", this);
                enabled = false;
                return;
            }

            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.CreatePool(config.TopPipePrefab, 5);
                ObjectPool.Instance.CreatePool(config.BottomPipePrefab, 5);
                ObjectPool.Instance.CreatePool(config.BranchPipePrefab, 5);
                if (config.ItemPrefab != null)
                {
                    ObjectPool.Instance.CreatePool(config.ItemPrefab, 10);
                }
            }

            if (config.TopPipePrefab == null || config.BottomPipePrefab == null || config.BranchPipePrefab == null)
            {
                Debug.LogError("PipeSpawner: 파이프 프리팹 설정이 누락되었습니다 (Top/Bottom/Branch).", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!_isSpawning)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            if (CurrentScrollSpeed < config.MaxMoveSpeed)
            {
                CurrentScrollSpeed += config.Acceleration * deltaTime;
                CurrentScrollSpeed = Mathf.Min(CurrentScrollSpeed, config.MaxMoveSpeed);
            }

            float movedDistanceThisFrame = CurrentScrollSpeed * deltaTime;
            if (_spawnScheduler.TryConsume(movedDistanceThisFrame, _spawnIntervalDistance))
            {
                SpawnObstaclePattern(config.PipeSpawnX, true);
            }

            _moverRegistry.CleanupInactiveMovers();
        }

        public void PreparePipes(bool preserveSpeed = false)
        {
            if (config == null)
            {
                return;
            }

            _prevItemY = null;
            _lastSpawnedItem = null;
            _consecutiveItemCount = 0;

            _patternGenerator.Reset(config);
            _spawnScheduler.Reset();

            if (!preserveSpeed)
            {
                CurrentScrollSpeed = config.PipeMoveSpeed;
            }

            ClearPipes();
            PreWarmPipes(false);
        }

        public void StartSpawning()
        {
            _isSpawning = true;
            IsScrolling = true;
            _moverRegistry.SetMovementState(true);
        }

        public void StopSpawning()
        {
            _isSpawning = false;
        }

        public void StopPipeMovement()
        {
            IsScrolling = false;
            _moverRegistry.SetMovementState(false);
        }

        public void ClearPipes()
        {
            _moverRegistry.Clear();
        }

        private void PreWarmPipes(bool moveImmediately)
        {
            if (config == null)
            {
                return;
            }

            if (_spawnIntervalDistance <= 0f)
            {
                _spawnIntervalDistance = config.PipeMoveSpeed * config.PipeSpawnInterval;
            }

            float pipeSpacing = _spawnIntervalDistance;
            if (pipeSpacing <= 0.01f)
            {
                Debug.LogError("[PipeSpawner] PipeSpacing이 0이거나 너무 작습니다. 무한 루프를 방지하기 위해 PreWarm을 중단합니다.");
                return;
            }

            float currentX = config.PipeSpawnX;
            List<float> spawnPositions = new List<float>();

            while (currentX >= -0.8f)
            {
                spawnPositions.Add(currentX);
                currentX -= pipeSpacing;
            }

            spawnPositions.Reverse();
            foreach (float x in spawnPositions)
            {
                SpawnObstaclePattern(x, moveImmediately);
            }
        }

        private void SpawnObstaclePattern(float spawnX, bool moveImmediately)
        {
            PipePatternResult pattern = _patternGenerator.Next(config);
            float centerY = pattern.CenterY;
            bool isBranching = pattern.IsBranching;

            if (_prevItemY.HasValue)
            {
                float halfDistance = (config.PipeMoveSpeed * config.PipeSpawnInterval) / 2f;
                float midX = spawnX - halfDistance;
                float midY = (_prevItemY.Value + centerY) / 2f;
                CreateItemObject(new Vector3(midX, midY, 0f), moveImmediately);
            }

            if (isBranching)
            {
                CreateBranchingPipes(centerY, spawnX, moveImmediately);

                float innerEdge = config.InnerPipeSize / 2f;
                float outerEdge = config.DoublePipeVerticalSpacing - (config.PipeSize / 2f);
                float gapCenterOffset = (innerEdge + outerEdge) / 2f + 0.5f;
                float offset = config.ItemPathSpacing / 2f;

                CreateItemObject(new Vector3(spawnX + offset, centerY + gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX, centerY + gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX - offset, centerY + gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX + offset, centerY - gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX, centerY - gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX - offset, centerY - gapCenterOffset, 0), moveImmediately);
            }
            else
            {
                CreateStandardPipePair(centerY, spawnX, moveImmediately);
                float offset = config.ItemPathSpacing / 2f;
                CreateItemObject(new Vector3(spawnX - offset, centerY, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX, centerY, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX + offset, centerY, 0), moveImmediately);
            }

            _prevItemY = centerY;
        }

        private void CreateStandardPipePair(float centerY, float spawnX, bool moveImmediately)
        {
            float halfGap = config.GapHeight / 2f;
            CreatePipeInstance(config.BottomPipePrefab, new Vector3(spawnX, centerY - halfGap, 0), moveImmediately);
            CreatePipeInstance(config.TopPipePrefab, new Vector3(spawnX, centerY + halfGap, 0), moveImmediately);
        }

        private void CreateBranchingPipes(float centerY, float spawnX, bool moveImmediately)
        {
            CreatePipeInstance(config.BranchPipePrefab, new Vector3(spawnX, centerY, 0), moveImmediately);
            float spacing = config.DoublePipeVerticalSpacing;
            CreatePipeInstance(config.BottomPipePrefab, new Vector3(spawnX, centerY - spacing, 0), moveImmediately);
            CreatePipeInstance(config.TopPipePrefab, new Vector3(spawnX, centerY + spacing, 0), moveImmediately);
        }

        private void CreatePipeInstance(GameObject prefab, Vector3 position, bool moveImmediately)
        {
            if (prefab == null || ObjectPool.Instance == null)
            {
                return;
            }

            GameObject pipeInstance = ObjectPool.Instance.Spawn(prefab, position, Quaternion.identity);
            pipeInstance.transform.SetParent(transform);
            pipeInstance.tag = TAG_PIPE;

            AttachComponents(pipeInstance, prefab, moveImmediately);
        }

        private void CreateItemObject(Vector3 position, bool moveImmediately)
        {
            if (config.ItemPrefab is null || ObjectPool.Instance == null)
            {
                return;
            }

            if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0)
            {
                return;
            }

            GameObject itemInstance = ObjectPool.Instance.Spawn(config.ItemPrefab, position, Quaternion.identity);
            itemInstance.transform.SetParent(transform);
            itemInstance.tag = TAG_ITEM;

            Item selectedItem = SelectNextItem();
            if (!itemInstance.TryGetComponent(out WorldItem worldItem))
            {
                worldItem = itemInstance.AddComponent<WorldItem>();
            }
            worldItem.Initialize(selectedItem);

            AttachComponents(itemInstance, config.ItemPrefab, moveImmediately);
        }

        private Item SelectNextItem()
        {
            int randomIndex = Random.Range(0, ItemDataBase.Items.Length);
            Item selectedItem = ItemDataBase.Items[randomIndex];

            if (ItemDataBase.Items.Length > 1)
            {
                if (selectedItem == _lastSpawnedItem)
                {
                    if (_consecutiveItemCount >= 2)
                    {
                        randomIndex = (randomIndex + 1) % ItemDataBase.Items.Length;
                        selectedItem = ItemDataBase.Items[randomIndex];
                        _lastSpawnedItem = selectedItem;
                        _consecutiveItemCount = 1;
                    }
                    else
                    {
                        _consecutiveItemCount++;
                    }
                }
                else
                {
                    _lastSpawnedItem = selectedItem;
                    _consecutiveItemCount = 1;
                }
            }

            return selectedItem;
        }

        private void AttachComponents(GameObject obj, GameObject originalPrefab, bool moveImmediately)
        {
            if (!obj.TryGetComponent(out LinearMover mover))
            {
                mover = obj.AddComponent<LinearMover>();
            }

            float initialSpeed = moveImmediately ? config.PipeMoveSpeed : 0f;
            mover.Initialize(Vector3.left, initialSpeed, this);

            if (!obj.TryGetComponent(out BoundaryRecycler recycler))
            {
                recycler = obj.AddComponent<BoundaryRecycler>();
            }

            float thresholdX = -config.PipeSpawnX - 5.0f;
            recycler.Initialize(thresholdX, originalPrefab);

            _moverRegistry.Register(mover, obj, originalPrefab);
        }
    }
}
