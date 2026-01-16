using FlappyBird.Configs;
using FlappyBird.Components;
using UnityEngine;
using Utils;
using System.Collections; // IEnumerator 사용을 위해 추가
using System.Collections.Generic;

namespace FlappyBird
{
    // 파이프와 아이템의 주기적인 생성을 담당하는 클래스입니다.
    public class PipeSpawner : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private FlappyBirdConfig config;

        [Header("프리팹")]
        [SerializeField] private GameObject pipePrefab;

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
            if (config == null || pipePrefab == null)
            {
                Debug.LogError("PipeSpawner: 설정 파일이나 파이프 프리팹이 누락되었습니다.", this);
                enabled = false;
                return;
            }
            
            // 오브젝트 풀링 미사용: Instantiate/Destroy 방식 사용
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
                
                float innerEdge = config.InnerPipeSize / 2f;
                float outerEdge = config.DoublePipeVerticalSpacing - (config.PipeSize / 2f);
                float gapCenterOffset = (innerEdge + outerEdge) / 2f;
                
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
            CreatePipeObject(centerY - halfGap, false, config.PipeSize);
            CreatePipeObject(centerY + halfGap, true, config.PipeSize);
        }

        private void CreateBranchingPipes(float centerY)
        {
            CreatePipeObject(centerY, false, config.InnerPipeSize); 
            float spacing = config.DoublePipeVerticalSpacing;
            CreatePipeObject(centerY - spacing, false, config.PipeSize);
            CreatePipeObject(centerY + spacing, true, config.PipeSize);
        }

        private void CreatePipeObject(float yPos, bool isTop, float scaleY)
        {
            Vector3 spawnPos = new Vector3(config.PipeSpawnX, yPos, 0);
            Quaternion rotation = isTop ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;

            GameObject pipeInstance = Instantiate(pipePrefab, spawnPos, rotation, transform);
            pipeInstance.tag = TAG_PIPE; 

            Vector3 targetScale = pipeInstance.transform.localScale;
            targetScale.y = scaleY; 
            pipeInstance.transform.localScale = targetScale;

            AttachComponents(pipeInstance, pipePrefab);
        }

        private void CreateItemObject(Vector3 position)
        {
            if (config.ItemPrefab == null) return;

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

            AttachComponents(itemInstance, config.ItemPrefab);
        }

        private void AttachComponents(GameObject obj, GameObject originalPrefab)
        {
            // 최적화: GetComponent 대신 TryGetComponent 사용 및 불필요한 AddComponent 호출 최소화
            // 프리팹에 미리 컴포넌트를 추가해두는 것이 성능상 가장 좋습니다.
            
            if (!obj.TryGetComponent(out LinearMover mover))
            {
                mover = obj.AddComponent<LinearMover>();
            }
            // 매번 초기화할 필요가 있는지 확인 (값이 변하지 않는다면 생략 가능하지만 안전을 위해 유지)
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
