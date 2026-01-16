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
            
            // 최적화: 초기 풀 크기를 줄여서 시작 시 프리징 현상 완화
            // 화면에 보이는 파이프 개수는 대략 5~6개이므로 10개면 충분합니다.
            ObjectPool.Instance.CreatePool(pipePrefab, 10);
            
            if (config.ItemPrefab != null)
            {
                ObjectPool.Instance.CreatePool(config.ItemPrefab, 10);
            }
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
                obj.transform.localScale = Vector3.one;
                obj.gameObject.SetActive(false); 
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

            GameObject pipeInstance = ObjectPool.Instance.Spawn(pipePrefab, spawnPos, rotation);
            pipeInstance.tag = TAG_PIPE; 

            Vector3 targetScale = pipeInstance.transform.localScale;
            targetScale.y = scaleY; 
            pipeInstance.transform.localScale = targetScale;

            AttachComponents(pipeInstance, pipePrefab);
        }

        private void CreateItemObject(Vector3 position)
        {
            if (config.ItemPrefab == null) return;

            GameObject itemInstance = ObjectPool.Instance.Spawn(config.ItemPrefab, position, Quaternion.identity);
            itemInstance.tag = TAG_ITEM;
            AttachComponents(itemInstance, config.ItemPrefab);
        }

        private void AttachComponents(GameObject obj, GameObject originalPrefab)
        {
            LinearMover mover = obj.GetComponent<LinearMover>();
            if (mover == null) mover = obj.AddComponent<LinearMover>();
            mover.Initialize(Vector3.left, config.PipeMoveSpeed);

            BoundaryRecycler recycler = obj.GetComponent<BoundaryRecycler>();
            if (recycler == null) recycler = obj.AddComponent<BoundaryRecycler>();

            float thresholdX = -config.PipeSpawnX - 5.0f;
            recycler.Initialize(thresholdX, originalPrefab);
        }
    }
}
