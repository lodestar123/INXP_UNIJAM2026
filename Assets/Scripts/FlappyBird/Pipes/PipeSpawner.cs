using FlappyBird.Configs;
using FlappyBird.Components;
using UnityEngine;
using Utils;
using System.Collections.Generic;

namespace FlappyBird
{
    // 파이프 생성 및 파이프 사이를 잇는 아이템 경로(Path) 생성을 담당하는 클래스입니다.
    public class PipeSpawner : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private FlappyBirdConfig config;

        [Header("프리팹")]
        [SerializeField] private GameObject pipePrefab;

        private bool _isSpawning = false;
        private float _timer = 0f;
        
        // 이전 패턴의 '통로(Gap)' 중심점 리스트 (1개 또는 2개)
        private List<float> _prevGapCenters = new List<float>();
        
        // 마지막으로 생성된 패턴의 중심 Y 좌표 (다음 패턴 위치 계산용)
        private float _lastPatternCenterY;

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
            
            InitializeObjectPools();
        }

        private void InitializeObjectPools()
        {
            ObjectPool.Instance.CreatePool(pipePrefab, 20);
            if (config.ItemPrefab != null)
            {
                ObjectPool.Instance.CreatePool(config.ItemPrefab, 100);
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
            
            // 초기화: 화면 중앙에서 시작한다고 가정
            float initialCenter = (config.PipeMinY + config.PipeMaxY) / 2f;
            _lastPatternCenterY = initialCenter;
            
            _prevGapCenters.Clear();
            _prevGapCenters.Add(initialCenter);
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

            // 1. 다음 패턴의 중심 위치 결정
            if (isBranching)
            {
                // 갈림길은 중앙 부근에서만 생성 (전체 가동 범위의 20% 이내)
                float centerY = (config.PipeMinY + config.PipeMaxY) / 2f;
                float centerRange = (config.PipeMaxY - config.PipeMinY) * 0.2f; 
                nextPatternCenterY = Random.Range(centerY - centerRange, centerY + centerRange);
            }
            else
            {
                // 일반 패턴은 이전 위치 기준 연속성 있게 생성
                nextPatternCenterY = CalculateNextSpawnHeight();
            }
            _lastPatternCenterY = nextPatternCenterY;

            // 2. 현재 패턴의 통로(Gap) 위치들 계산
            List<float> currentGapCenters = new List<float>();
            
            if (isBranching)
            {
                CreateBranchingPipes(nextPatternCenterY);
                
                // [수정] 아이템 경로 중심점 계산 로직 변경
                // 기존: VerticalSpacing / 2 (설정값에 의존적, 장애물 크기 무시)
                // 변경: (장애물 반 크기) + (통로 반 크기) -> 장애물 표면에서 적정 거리만큼 떨어진 곳
                
                // 중앙 장애물의 위쪽 끝 + Gap의 절반
                float topGapCenter = nextPatternCenterY + (config.InnerPipeSize / 2f) + (config.GapHeight / 2f);
                
                // 중앙 장애물의 아래쪽 끝 - Gap의 절반
                float bottomGapCenter = nextPatternCenterY - (config.InnerPipeSize / 2f) - (config.GapHeight / 2f);

                currentGapCenters.Add(topGapCenter);    // 위쪽 통로
                currentGapCenters.Add(bottomGapCenter); // 아래쪽 통로
            }
            else
            {
                CreateStandardPipePair(nextPatternCenterY);
                currentGapCenters.Add(nextPatternCenterY); // 단일 통로
            }

            // 3. 아이템 경로 생성 및 연결
            ConnectGapsWithItems(_prevGapCenters, currentGapCenters);

            // 4. 현재 정보를 다음 루프를 위해 저장
            _prevGapCenters = currentGapCenters;
        }

        private float CalculateNextSpawnHeight()
        {
            float variance = Random.Range(-config.PipeHeightVariance, config.PipeHeightVariance);
            float newY = _lastPatternCenterY + variance;
            return Mathf.Clamp(newY, config.PipeMinY, config.PipeMaxY);
        }

        private void ConnectGapsWithItems(List<float> prevGaps, List<float> nextGaps)
        {
            if (prevGaps.Count == 0 || nextGaps.Count == 0) return;

            // 일반 -> 일반
            if (prevGaps.Count == 1 && nextGaps.Count == 1)
            {
                CreateItemPath(prevGaps[0], nextGaps[0]);
            }
            // 일반 -> 갈림길 (분기)
            else if (prevGaps.Count == 1 && nextGaps.Count == 2)
            {
                CreateItemPath(prevGaps[0], nextGaps[0]);
                CreateItemPath(prevGaps[0], nextGaps[1]);
            }
            // 갈림길 -> 일반 (합류)
            else if (prevGaps.Count == 2 && nextGaps.Count == 1)
            {
                CreateItemPath(prevGaps[0], nextGaps[0]);
                CreateItemPath(prevGaps[1], nextGaps[0]);
            }
            // 갈림길 -> 갈림길 (평행)
            else if (prevGaps.Count == 2 && nextGaps.Count == 2)
            {
                CreateItemPath(prevGaps[0], nextGaps[0]); 
                CreateItemPath(prevGaps[1], nextGaps[1]);
            }
        }

        private void CreateItemPath(float startY, float endY)
        {
            if (config.ItemPrefab == null) return;

            float distanceBetweenPipes = config.PipeMoveSpeed * config.PipeSpawnInterval;
            
            // 시작점: 이전 파이프의 구멍 위치
            Vector3 startPoint = new Vector3(config.PipeSpawnX - distanceBetweenPipes, startY, 0);
            // 끝점: 현재 생성된 파이프의 구멍 위치
            Vector3 endPoint = new Vector3(config.PipeSpawnX, endY, 0);

            float totalDistance = Vector3.Distance(startPoint, endPoint);
            int itemCount = Mathf.Max(1, Mathf.FloorToInt(totalDistance / config.ItemPathSpacing));

            // i=1 부터 itemCount까지 반복하여 끝점(t=1.0)을 포함시킴
            // 이렇게 하면 파이프 통로 한가운데에 아이템이 정확히 생성됩니다.
            for (int i = 1; i <= itemCount; i++)
            {
                float t = (float)i / itemCount; 
                Vector3 spawnPos = Vector3.Lerp(startPoint, endPoint, t);
                CreateItemObject(spawnPos);
            }
        }

        // --- 파이프 생성 로직 ---

        private void CreateStandardPipePair(float centerY)
        {
            float halfGap = config.GapHeight / 2f;
            CreatePipeObject(centerY - halfGap, false, config.PipeSize);
            CreatePipeObject(centerY + halfGap, true, config.PipeSize);
        }

        private void CreateBranchingPipes(float centerY)
        {
            // 중앙 장애물 (InnerPipeSize 사용)
            CreatePipeObject(centerY, false, config.InnerPipeSize); 
            
            // [수정] 위/아래 파이프 배치 간격 계산
            // 기존 Spacing 대신 (중앙 파이프 반) + (Gap) + (외부 파이프 반? 아니면 그냥 갭만?)
            // 기획 의도상 "GapHeight"는 통로의 크기여야 합니다.
            // 따라서 중앙 장애물 끝에서 GapHeight만큼 떨어진 곳에 위/아래 파이프가 시작되어야 합니다.
            // 하지만 PipeObject의 좌표는 '중심' 기준이므로 계산이 필요합니다.
            
            // 중앙 파이프의 위쪽 끝 = centerY + (InnerPipeSize / 2)
            // 통로 공간 = GapHeight
            // 위쪽 파이프의 아래쪽 끝 = (중앙 위쪽 끝) + GapHeight
            // 위쪽 파이프의 중심 = (위쪽 파이프 아래쪽 끝) + (PipeSize / 2)
            
            // => centerY + (InnerPipeSize / 2) + GapHeight + (PipeSize / 2)
            // (이전 로직은 Spacing 변수를 썼지만, Spacing과 Gap의 관계가 모호했음)
            
            // 여기서는 안전하게 'DoublePipeVerticalSpacing' 설정을 존중하되, 
            // 아이템 경로 계산은 실제 물리적 빈 공간(Gap)을 기준으로 하도록 위에서 수정했습니다.
            // 파이프 배치 자체는 Config의 Spacing 값을 따릅니다.
            // *만약 Spacing 설정값이 너무 작으면 물리적으로 파이프가 겹칠 수 있습니다.*
            
            float spacing = config.DoublePipeVerticalSpacing;

            // 위/아래 파이프 (PipeSize 사용)
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