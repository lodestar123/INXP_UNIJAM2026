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

        private bool _isSpawning = false;
        private float _timer = 0f;
        private float _lastPatternCenterY;

        // 이전 패턴 아이템의 대표 Y 좌표
        private float? _prevItemY = null;
        private bool _wasLastPatternBranching = false;

        // 아이템 중복 방지 로직을 위한 변수
        private Item _lastSpawnedItem;
        private int _consecutiveItemCount = 0;

        // 첫 번째 파이프 갈래길 방지 로직
        private int _spawnedPatternCount = 0;

        private const string TAG_PIPE = "Pipe";
        private const string TAG_ITEM = "Item";

        private void Start()
        {
            if (config == null)
            {
                Debug.LogError("PipeSpawner: 설정 파일이 누락되었습니다.", this);
                enabled = false;
                return;
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

            _timer += Time.deltaTime;
            if (_timer >= config.PipeSpawnInterval)
            {
                _timer = 0f;
                SpawnObstaclePattern(config.PipeSpawnX, true);
            }
        }

        // 게임 준비 단계(Ready)에서 호출되어, 화면에 파이프를 미리 배치합니다. (움직이지 않음)
        public void PreparePipes()
        {
            if (config == null) return;

            _lastPatternCenterY = (config.PipeMinY + config.PipeMaxY) / 2f;
            _prevItemY = null;
            _wasLastPatternBranching = false;

            // 아이템 중복 카운터 초기화
            _lastSpawnedItem = null;
            _consecutiveItemCount = 0;

            // 패턴 카운트 초기화
            _spawnedPatternCount = 0;

            // 움직이지 않는 상태로 미리 생성
            PreWarmPipes(false);
        }

        // 게임 시작 시 호출되어, 생성된 파이프들을 움직이기 시작하고 추가 생성을 시작합니다.
        public void StartSpawning()
        {
            _isSpawning = true;
            _timer = 0f;

            // 이미 생성된 모든 파이프/아이템의 이동 시작
            LinearMover[] movers = FindObjectsByType<LinearMover>(FindObjectsSortMode.None);
            foreach (var mover in movers)
            {
                mover.Initialize(Vector3.left, config.PipeMoveSpeed);
            }
        }

        public void StopSpawning()
        {
            _isSpawning = false;
        }

        public void StopPipeMovement()
        {
            var movers = FindObjectsByType<LinearMover>(FindObjectsSortMode.None);
            foreach (var mover in movers)
            {
                // 속도를 0으로 설정하여 멈춤 (방향은 유지)
                mover.Initialize(Vector3.left, 0f);
            }
        }

        public void ClearPipes()
        {
            BoundaryRecycler[] activeObjects = FindObjectsByType<BoundaryRecycler>(FindObjectsSortMode.None);
            foreach (BoundaryRecycler obj in activeObjects)
            {
                Destroy(obj.gameObject);
            }
        }

        private void PreWarmPipes(bool moveImmediately)
        {
            float pipeSpacing = config.PipeMoveSpeed * config.PipeSpawnInterval;
            float currentX = config.PipeSpawnX;

            // 생성할 X 좌표들을 리스트에 담습니다. (오른쪽 -> 왼쪽 역순 탐색)
            List<float> spawnPositions = new List<float>();
            while (currentX >= -0.8f)
            {
                spawnPositions.Add(currentX);
                currentX -= pipeSpacing;
            }

            // 왼쪽에서 오른쪽 순서로 생성해야 아이템 연결 로직이 정상 작동합니다.
            spawnPositions.Reverse();

            foreach (float x in spawnPositions)
            {
                SpawnObstaclePattern(x, moveImmediately);
            }
        }

        private void SpawnObstaclePattern(float spawnX, bool moveImmediately)
        {
            // 갈림길이 연속으로 나오지 않도록 체크
            bool isBranching = Random.value < config.DoublePipeChance;
            if (_wasLastPatternBranching) isBranching = false;

            // 첫 번째 패턴은 절대 갈래길이 아님
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

            // 이전 파이프와 현재 파이프 사이에 아이템 생성
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

                // 아이템 배치를 위한 오프셋 계산
                float innerEdge = config.InnerPipeSize / 2f;
                float outerEdge = config.DoublePipeVerticalSpacing - (config.PipeSize / 2f);
                float gapCenterOffset = (innerEdge + outerEdge) / 2f + 0.5f; // 약간 보정

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
                // 파이프 하나당 두 개씩 아이템 생성
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

            // Bottom Pipe
            CreatePipeInstance(config.BottomPipePrefab, new Vector3(spawnX, centerY - halfGap, 0), moveImmediately);

            // Top Pipe
            CreatePipeInstance(config.TopPipePrefab, new Vector3(spawnX, centerY + halfGap, 0), moveImmediately);
        }

        private void CreateBranchingPipes(float centerY, float spawnX, bool moveImmediately)
        {
            // Center Pipe
            CreatePipeInstance(config.BranchPipePrefab, new Vector3(spawnX, centerY, 0), moveImmediately);

            float spacing = config.DoublePipeVerticalSpacing;

            // Bottom Pipe
            CreatePipeInstance(config.BottomPipePrefab, new Vector3(spawnX, centerY - spacing, 0), moveImmediately);

            // Top Pipe
            CreatePipeInstance(config.TopPipePrefab, new Vector3(spawnX, centerY + spacing, 0), moveImmediately);
        }

        private void CreatePipeInstance(GameObject prefab, Vector3 position, bool moveImmediately)
        {
            if (prefab is null) return;

            GameObject pipeInstance = Instantiate(prefab, position, Quaternion.identity, transform);
            pipeInstance.tag = TAG_PIPE;

            AttachComponents(pipeInstance, moveImmediately);
        }

        private void CreateItemObject(Vector3 position, bool moveImmediately)
        {
            if (config.ItemPrefab == null) return;

            if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0)
            {
                Debug.LogWarning("[PipeSpawner] ItemDataBase에 아이템이 없습니다.");
                return;
            }

            GameObject itemInstance = Instantiate(config.ItemPrefab, position, Quaternion.identity, transform);
            itemInstance.tag = TAG_ITEM;

            // 아이템 선택 로직 (3회 이상 연속 중복 방지)
            int randomIndex = Random.Range(0, ItemDataBase.Items.Length);
            Item selectedItem = ItemDataBase.Items[randomIndex];

            if (ItemDataBase.Items.Length > 1) // 아이템 종류가 2개 이상일 때만 로직 적용
            {
                if (selectedItem == _lastSpawnedItem)
                {
                    if (_consecutiveItemCount >= 2)
                    {
                        // 이미 2번 연속 나왔다면, 다음 아이템(인덱스+1)을 선택하여 강제로 변경
                        randomIndex = (randomIndex + 1) % ItemDataBase.Items.Length;
                        selectedItem = ItemDataBase.Items[randomIndex];

                        // 변경되었으므로 카운트 초기화 (새로운 아이템 1회차)
                        _lastSpawnedItem = selectedItem;
                        _consecutiveItemCount = 1;
                    }
                    else
                    {
                        // 연속이지만 아직 제한 미만
                        _consecutiveItemCount++;
                    }
                }
                else
                {
                    // 다른 아이템이 나왔으므로 리셋
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

            // Ready 상태에서는 0의 속도, 게임 시작 시에는 설정된 속도로 초기화
            float speed = moveImmediately ? config.PipeMoveSpeed : 0f;
            mover.Initialize(Vector3.left, speed);

            if (!obj.TryGetComponent(out BoundaryRecycler recycler))
            {
                recycler = obj.AddComponent<BoundaryRecycler>();
            }
            float thresholdX = -config.PipeSpawnX - 5.0f;

            recycler.Initialize(thresholdX, null);
        }
    }
}