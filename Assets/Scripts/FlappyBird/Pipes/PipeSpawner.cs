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

        // 기존 pipePrefab 필드는 제거됨 (Config의 Top/Bottom/Branch Prefab 사용)

        private bool _isSpawning = false;
        private float _timer = 0f;
        private float _lastPatternCenterY;
        
        // 이전 패턴 아이템의 대표 Y 좌표
        private float? _prevItemY = null;
        private bool _wasLastPatternBranching = false;

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

            if (config.TopPipePrefab != null && config.BottomPipePrefab != null &&
                config.BranchPipePrefab != null) return;
            Debug.LogError("PipeSpawner: 파이프 프리팹 설정이 누락되었습니다 (Top/Bottom/Branch).", this);
            enabled = false;
        }

        private void Update()
        {
            if (!_isSpawning) return;

            _timer += Time.deltaTime;
            if (_timer >= config.PipeSpawnInterval)
            {
                _timer = 0f;
                SpawnObstaclePattern();
            }
        }

        public void StartSpawning()
        {
            _isSpawning = true;
            _timer = 0f;
            _lastPatternCenterY = (config.PipeMinY + config.PipeMaxY) / 2f;
            _prevItemY = null;
            _wasLastPatternBranching = false;
            
            // 시작 즉시 첫 번째 패턴 생성 (화면 진입 전 미리 생성)
            SpawnObstaclePattern();
        }

        public void StopSpawning()
        {
            _isSpawning = false;
        }

        public void ClearPipes()
        {
            BoundaryRecycler[] activeObjects = FindObjectsByType<BoundaryRecycler>(FindObjectsSortMode.None);
            foreach (BoundaryRecycler obj in activeObjects)
            {
                Destroy(obj.gameObject); 
            }
        }

        private void SpawnObstaclePattern()
        {
            // 갈림길이 연속으로 나오지 않도록 체크
            bool isBranching = Random.value < config.DoublePipeChance;
            if (_wasLastPatternBranching) isBranching = false;

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
                float midX = config.PipeSpawnX - halfDistance;
                float midY = (_prevItemY.Value + currentItemAvgY) / 2f;

                CreateItemObject(new Vector3(midX, midY, 0));
            }

            if (isBranching)
            {
                CreateBranchingPipes(nextPatternCenterY);
                
                // 아이템 배치를 위한 오프셋 계산 (기존 PipeSize 필드를 기준값으로 사용)
                float innerEdge = config.InnerPipeSize / 2f;
                float outerEdge = config.DoublePipeVerticalSpacing - (config.PipeSize / 2f);
                float gapCenterOffset = (innerEdge + outerEdge) / 2f + 0.5f;
                
                float offset = config.ItemPathSpacing / 2f;
                
                CreateItemObject(new Vector3(config.PipeSpawnX + offset, nextPatternCenterY + gapCenterOffset, 0));
                CreateItemObject(new Vector3(config.PipeSpawnX, nextPatternCenterY + gapCenterOffset, 0));
                CreateItemObject(new Vector3(config.PipeSpawnX - offset, nextPatternCenterY + gapCenterOffset, 0));
                CreateItemObject(new Vector3(config.PipeSpawnX + offset, nextPatternCenterY - gapCenterOffset, 0));
                CreateItemObject(new Vector3(config.PipeSpawnX, nextPatternCenterY - gapCenterOffset, 0));
                CreateItemObject(new Vector3(config.PipeSpawnX - offset, nextPatternCenterY - gapCenterOffset, 0));
            }
            else
            {
                CreateStandardPipePair(nextPatternCenterY);
                // 파이프 하나당 두 개씩 아이템 생성
                float offset = config.ItemPathSpacing / 2f;
                CreateItemObject(new Vector3(config.PipeSpawnX - offset, nextPatternCenterY, 0));
                CreateItemObject(new Vector3(config.PipeSpawnX, nextPatternCenterY, 0));
                CreateItemObject(new Vector3(config.PipeSpawnX + offset, nextPatternCenterY, 0));
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

        private void CreateStandardPipePair(float centerY)
        {
            float halfGap = config.GapHeight / 2f;
            
            // Bottom Pipe (아래쪽 파이프 - 위로 솟은 파이프)
            CreatePipeInstance(config.BottomPipePrefab, new Vector3(config.PipeSpawnX, centerY - halfGap, 0));
            
            // Top Pipe (위쪽 파이프 - 아래로 뻗은 파이프)
            CreatePipeInstance(config.TopPipePrefab, new Vector3(config.PipeSpawnX, centerY + halfGap, 0));
        }

        private void CreateBranchingPipes(float centerY)
        {
            // Center Pipe (갈림길 중앙)
            CreatePipeInstance(config.BranchPipePrefab, new Vector3(config.PipeSpawnX, centerY, 0)); 
            
            float spacing = config.DoublePipeVerticalSpacing;
            
            // Bottom Pipe (아래쪽)
            CreatePipeInstance(config.BottomPipePrefab, new Vector3(config.PipeSpawnX, centerY - spacing, 0));
            
            // Top Pipe (위쪽)
            CreatePipeInstance(config.TopPipePrefab, new Vector3(config.PipeSpawnX, centerY + spacing, 0));
        }

        private void CreatePipeInstance(GameObject prefab, Vector3 position)
        {
            if (prefab is null) return;

            GameObject pipeInstance = Instantiate(prefab, position, Quaternion.identity, transform);
            pipeInstance.tag = TAG_PIPE; 

            // 스케일 조절 없음 (프리팹 설정 유지)
            
            AttachComponents(pipeInstance);
        }

        private void CreateItemObject(Vector3 position)
        {
            if (config.ItemPrefab is null) return;

            // ItemDataBase에서 랜덤 아이템 선택
            if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0)
            {
                Debug.LogWarning("[PipeSpawner] ItemDataBase에 아이템이 없습니다.");
                return;
            }

            GameObject itemInstance = Instantiate(config.ItemPrefab, position, Quaternion.identity, transform);
            itemInstance.tag = TAG_ITEM;

            // 랜덤 아이템 데이터 설정
            int randomIndex = Random.Range(0, ItemDataBase.Items.Length);
            Item randomItem = ItemDataBase.Items[randomIndex];
            
            if (!itemInstance.TryGetComponent(out WorldItem worldItem))
            {
                worldItem = itemInstance.AddComponent<WorldItem>();
            }
            worldItem.Initialize(randomItem);

            AttachComponents(itemInstance);
        }

        private void AttachComponents(GameObject obj)
        {
            // 최적화: GetComponent 대신 TryGetComponent 사용 및 불필요한 AddComponent 호출 최소화
            
            if (!obj.TryGetComponent(out LinearMover mover))
            {
                mover = obj.AddComponent<LinearMover>();
            }
            // 매번 초기화할 필요가 있는지 확인
            mover.Initialize(Vector3.left, config.PipeMoveSpeed);

            if (!obj.TryGetComponent(out BoundaryRecycler recycler))
            {
                recycler = obj.AddComponent<BoundaryRecycler>();
            }
            float thresholdX = -config.PipeSpawnX - 5.0f;
            
            // null을 전달하여 BoundaryRecycler가 Destroy()를 호출하도록 유도
            recycler.Initialize(thresholdX, null);
        }
    }
}