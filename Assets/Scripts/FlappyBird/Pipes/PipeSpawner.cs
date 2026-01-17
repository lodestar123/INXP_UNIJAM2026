using FlappyBird.Configs;
using FlappyBird.Components;
using UnityEngine;
using Utils;
using System.Collections;
using System.Collections.Generic;

namespace FlappyBird
{
    // 파이프와 아이템의 주기적인 생성을 담당하는 클래스입니다.
    public class PipeSpawner : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private FlappyBirdConfig config;

        public static float CurrentScrollSpeed { get; private set; }

        private bool _isSpawning = false;
        private float _movedDistance = 0f;
        private float _spawnIntervalDistance = 0f;

        private float _lastPatternCenterY;
        
        private float? _prevItemY = null;
        private bool _wasLastPatternBranching = false;

        private Item _lastSpawnedItem;
        private int _consecutiveItemCount = 0;

        private int _spawnedPatternCount = 0;

        private const string TAG_PIPE = "Pipe";
        private const string TAG_ITEM = "Item";
        
        // [최적화 1] 씬 전체 검색(FindObjectsByType)을 대체하기 위한 활성 객체 리스트
        private List<LinearMover> _activeMovers = new List<LinearMover>();

        // [최적화 2] ObjectPool.Return시 원본 Prefab 정보가 필요하므로 이를 추적하는 딕셔너리 (Instance -> Prefab)
        private Dictionary<GameObject, GameObject> _instanceToPrefabMap = new Dictionary<GameObject, GameObject>();

        private void Awake()
        {
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
            if (!_isSpawning) return;

            float deltaTime = Time.deltaTime;

            // 1. 속도 가속 로직 (최대 속도까지 증가)
            if (CurrentScrollSpeed < config.MaxMoveSpeed)
            {
                CurrentScrollSpeed += config.Acceleration * deltaTime;
                CurrentScrollSpeed = Mathf.Min(CurrentScrollSpeed, config.MaxMoveSpeed);
            }

            // 2. 거리 기반 스폰 로직
            // 이번 프레임에 이동한 거리만큼 누적
            _movedDistance += CurrentScrollSpeed * deltaTime;

            // 누적 이동 거리가 목표 간격보다 커지면 파이프 생성
            if (_movedDistance >= _spawnIntervalDistance)
            {
                // 누적된 거리에서 목표 간격을 뺍니다 (0으로 초기화하면 오차가 쌓임)
                _movedDistance -= _spawnIntervalDistance;
                SpawnObstaclePattern(config.PipeSpawnX, true);
            }
            
            CleanupInactiveMovers();
        }

        // 게임 준비 단계(Ready)에서 호출되어, 화면에 파이프를 미리 배치합니다. (움직이지 않음)
        public void PreparePipes()
        {
            if (config == null) return;

            _lastPatternCenterY = (config.PipeMinY + config.PipeMaxY) / 2f;
            _prevItemY = null;
            _wasLastPatternBranching = false;

            _lastSpawnedItem = null;
            _consecutiveItemCount = 0;
            _spawnedPatternCount = 0;

            // 패턴 카운트 초기화
            CurrentScrollSpeed = config.PipeMoveSpeed;
            _movedDistance = 0f;

            // 기존 파이프들 풀로 반환 및 정리
            ClearPipes();
            
            // 움직이지 않는 상태로 미리 생성
            PreWarmPipes(false);
        }

        // 게임 시작 시 호출되어, 생성된 파이프들을 움직이기 시작하고 추가 생성을 시작합니다.
        public void StartSpawning()
        {
            _isSpawning = true;

            // 이미 생성된 모든 파이프/아이템의 이동 시작
            LinearMover[] movers = FindObjectsByType<LinearMover>(FindObjectsSortMode.None);
            foreach (LinearMover mover in _activeMovers)
            {
                if (mover is not null && mover.gameObject.activeInHierarchy)
                {
                    mover.SetMoveState(true);
                }
            }
        }

        public void StopSpawning()
        {
            _isSpawning = false;
        }

        public void StopPipeMovement()
        {
            foreach (LinearMover mover in _activeMovers)
            {
                if (mover != null && mover.gameObject.activeInHierarchy)
                {
                    mover.SetMoveState(false);
                }
            }
        }

        public void ClearPipes()
        {
            for (int i = _activeMovers.Count - 1; i >= 0; i--)
            {
                LinearMover mover = _activeMovers[i];
                if (mover != null && mover.gameObject.activeSelf) // 이미 반환된 것은 제외
                {
                    ReturnToPool(mover.gameObject);
                }
            }
            
            _activeMovers.Clear();
            _instanceToPrefabMap.Clear();
        }
        
        private void ReturnToPool(GameObject instance)
        {
            if (_instanceToPrefabMap.TryGetValue(instance, out GameObject prefab))
            {
                ObjectPool.Instance.Return(prefab, instance);
            }
            else
            {
                // 매핑 정보가 없다면 그냥 파괴 (예외 상황)
                Destroy(instance);
            }
        }
        
        private void CleanupInactiveMovers()
        {
            // 리스트 역순 순회 삭제
            for (int i = _activeMovers.Count - 1; i >= 0; i--)
            {
                if (_activeMovers[i] is null || !_activeMovers[i].gameObject.activeInHierarchy)
                {
                    _activeMovers.RemoveAt(i);
                }
            }
        }

        private void PreWarmPipes(bool moveImmediately)
        {
            float pipeSpacing = _spawnIntervalDistance;
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
            bool isBranching = Random.value < config.DoublePipeChance;
            if (_wasLastPatternBranching) isBranching = false;
            if (_spawnedPatternCount == 0) isBranching = false;

            _spawnedPatternCount++;

            float nextPatternCenterY;

            if (isBranching)
            {
                nextPatternCenterY = (config.PipeMinY + config.PipeMaxY) / 2f;
            }
            else
            {
                nextPatternCenterY = CalculateNextSpawnHeight();
            }
            _lastPatternCenterY = nextPatternCenterY;

            float currentItemAvgY = nextPatternCenterY;

            if (_prevItemY.HasValue)
            {
                float halfDistance = (config.PipeMoveSpeed * config.PipeSpawnInterval) / 2f;
                float midX = spawnX - halfDistance;
                float midY = (_prevItemY.Value + currentItemAvgY) / 2f;

                CreateItemObject(new Vector3(midX, midY, 0), moveImmediately);
            }

            if (isBranching)
            {
                CreateBranchingPipes(nextPatternCenterY, spawnX, moveImmediately);

                float innerEdge = config.InnerPipeSize / 2f;
                float outerEdge = config.DoublePipeVerticalSpacing - (config.PipeSize / 2f);
                float gapCenterOffset = (innerEdge + outerEdge) / 2f + 0.5f;
                float offset = config.ItemPathSpacing / 2f;

                CreateItemObject(new Vector3(spawnX + offset, nextPatternCenterY + gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX, nextPatternCenterY + gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX - offset, nextPatternCenterY + gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX + offset, nextPatternCenterY - gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX, nextPatternCenterY - gapCenterOffset, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX - offset, nextPatternCenterY - gapCenterOffset, 0), moveImmediately);
            }
            else
            {
                CreateStandardPipePair(nextPatternCenterY, spawnX, moveImmediately);
                float offset = config.ItemPathSpacing / 2f;
                CreateItemObject(new Vector3(spawnX - offset, nextPatternCenterY, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX, nextPatternCenterY, 0), moveImmediately);
                CreateItemObject(new Vector3(spawnX + offset, nextPatternCenterY, 0), moveImmediately);
            }

            _prevItemY = currentItemAvgY;
            _wasLastPatternBranching = isBranching;
        }

        private float CalculateNextSpawnHeight()
        {
            float variance = Random.Range(-config.PipeHeightVariance, config.PipeHeightVariance);
            float newY = _lastPatternCenterY + variance;
            return Mathf.Clamp(newY, config.PipeMinY, config.PipeMaxY);
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
            if (prefab == null) return;

            // [최적화] ObjectPool 사용
            GameObject pipeInstance = ObjectPool.Instance.Spawn(prefab, position, Quaternion.identity);
            
            // 반환 시 매핑을 위해 Dictionary에 저장
            if (!_instanceToPrefabMap.ContainsKey(pipeInstance))
            {
                _instanceToPrefabMap.Add(pipeInstance, prefab);
            }

            pipeInstance.transform.SetParent(transform); // 필요시 부모 설정
            pipeInstance.tag = TAG_PIPE;

            AttachComponents(pipeInstance, moveImmediately);
        }

        private void CreateItemObject(Vector3 position, bool moveImmediately)
        {
            if (config.ItemPrefab is null) return;
            if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0) return;

            // [최적화] ObjectPool 사용
            GameObject itemInstance = ObjectPool.Instance.Spawn(config.ItemPrefab, position, Quaternion.identity);

            // 반환 시 매핑을 위해 Dictionary에 저장
            if (!_instanceToPrefabMap.ContainsKey(itemInstance))
            {
                _instanceToPrefabMap.Add(itemInstance, config.ItemPrefab);
            }

            itemInstance.transform.SetParent(transform);
            itemInstance.tag = TAG_ITEM;

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

            if (!itemInstance.TryGetComponent(out WorldItem worldItem))
            {
                worldItem = itemInstance.AddComponent<WorldItem>();
            }
            worldItem.Initialize(selectedItem);

            AttachComponents(itemInstance, moveImmediately);
        }

        private void AttachComponents(GameObject obj, bool moveImmediately)
        {
            if (!obj.TryGetComponent(out LinearMover mover))
            {
                mover = obj.AddComponent<LinearMover>();
            }

            // [최적화] 생성된 mover를 관리 리스트에 추가
            _activeMovers.Add(mover);

            float initialSpeed = moveImmediately ? config.PipeMoveSpeed : 0f;
            mover.Initialize(Vector3.left, initialSpeed);

            if (!obj.TryGetComponent(out BoundaryRecycler recycler))
            {
                recycler = obj.AddComponent<BoundaryRecycler>();
            }

            float thresholdX = -config.PipeSpawnX - 5.0f;
            
            // 주의: BoundaryRecycler도 ObjectPool.Return(prefab, instance)를 호출해야 합니다.
            // BoundaryRecycler 코드를 직접 볼 수는 없지만, 만약 내부에서 Destroy 대신 풀 반환을 한다면
            // 그곳에서도 원본 Prefab에 대한 참조가 필요할 것입니다.
            // 일단 Spawner에서는 recycler 초기화만 수행합니다.
            recycler.Initialize(thresholdX, null);
        }
    }
}