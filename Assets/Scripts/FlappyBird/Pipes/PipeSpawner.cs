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
            
            // 최적화: 풀 생성을 코루틴으로 분산 처리
            StartCoroutine(InitializeObjectPoolsRoutine());
        }

        private IEnumerator InitializeObjectPoolsRoutine()
        {
            // 파이프 풀 생성 (부하 분산)
            // ObjectPool 내부 구조를 모르므로, 안전하게 외부에서 제어하기보다
            // 일단은 메인 스레드 부하를 줄이기 위해 한 프레임 대기 후 실행하거나
            // 만약 ObjectPool.CreatePool이 시간이 걸린다면 여기서 쪼개야 하지만,
            // 현재 구조상 CreatePool 호출 자체를 딜레이시키는 것만으로도 시작 멈춤 현상은 완화됩니다.
            
            // 파이프 풀 생성
            ObjectPool.Instance.CreatePool(pipePrefab, 20);
            yield return null; // 한 프레임 쉬고

            // 아이템 풀 생성
            if (config.ItemPrefab != null)
            {
                ObjectPool.Instance.CreatePool(config.ItemPrefab, 20);
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
            bool isBranching = Random.value < config.DoublePipeChance;
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

                CreateItemObject(new Vector3(config.PipeSpawnX, nextPatternCenterY + gapCenterOffset, 0));
                CreateItemObject(new Vector3(config.PipeSpawnX, nextPatternCenterY - gapCenterOffset, 0));
            }
            else
            {
                CreateStandardPipePair(nextPatternCenterY);
                CreateItemObject(new Vector3(config.PipeSpawnX, nextPatternCenterY, 0));
            }

            _prevItemY = currentItemAvgY;
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
