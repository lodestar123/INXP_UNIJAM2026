using FlappyBird.Configs;
using FlappyBird.Components;
using UnityEngine;
using Utils;

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
        
        // 이전 파이프의 중심 Y 좌표 (경로 시작점)
        private float _prevCenterY;
        
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
                ObjectPool.Instance.CreatePool(config.ItemPrefab, 50);
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
            _prevCenterY = (config.PipeMinY + config.PipeMaxY) / 2f;
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
            float nextY;

            if (isBranching)
            {
                // 갈림길은 중앙 부근에서만 생성 (화면 전체 높이 범위의 20% 이내)
                float centerY = (config.PipeMinY + config.PipeMaxY) / 2f;
                float centerRange = (config.PipeMaxY - config.PipeMinY) * 0.2f; 
                nextY = Random.Range(centerY - centerRange, centerY + centerRange);
            }
            else
            {
                // 일반 패턴은 이전 위치를 기준으로 연속성 있게 생성
                nextY = CalculateNextSpawnHeight();
            }
            
            if (isBranching)
            {
                CreateBranchingPipes(nextY);
                // 갈림길: 이전 지점에서 위/아래 두 갈래로 나뉘는 아이템 경로 생성
                float spacing = config.DoublePipeVerticalSpacing;
                CreateItemPath(_prevCenterY, nextY + (spacing / 2f)); // 위쪽 길
                CreateItemPath(_prevCenterY, nextY - (spacing / 2f)); // 아래쪽 길
            }
            else
            {
                CreateStandardPipePair(nextY);
                // 일반: 이전 지점에서 다음 지점으로 이어지는 단일 경로 생성
                CreateItemPath(_prevCenterY, nextY);
            }

            _prevCenterY = nextY;
        }

        private float CalculateNextSpawnHeight()
        {
            float variance = Random.Range(-config.PipeHeightVariance, config.PipeHeightVariance);
            float newY = _prevCenterY + variance;
            return Mathf.Clamp(newY, config.PipeMinY, config.PipeMaxY);
        }

        // --- 파이프 생성 로직 ---

        private void CreateStandardPipePair(float centerY)
        {
            float halfGap = config.GapHeight / 2f;
            
            // 일반 파이프 크기 적용
            CreatePipeObject(centerY - halfGap, false, config.PipeSize);
            CreatePipeObject(centerY + halfGap, true, config.PipeSize);
        }

        private void CreateBranchingPipes(float centerY)
        {
            // 중앙 장애물 (작은 파이프 크기 적용)
            CreatePipeObject(centerY, false, config.InnerPipeSize); 
            
            float spacing = config.DoublePipeVerticalSpacing;

            // 위/아래 파이프 (일반 파이프 크기 적용)
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

        // --- 아이템 경로 생성 로직 ---

        private void CreateItemPath(float startY, float endY)
        {
            if (config.ItemPrefab == null) return;

            float distanceBetweenPipes = config.PipeMoveSpeed * config.PipeSpawnInterval;
            
            Vector3 startPoint = new Vector3(config.PipeSpawnX - distanceBetweenPipes, startY, 0);
            Vector3 endPoint = new Vector3(config.PipeSpawnX, endY, 0);

            float totalDistance = Vector3.Distance(startPoint, endPoint);
            int itemCount = Mathf.FloorToInt(totalDistance / config.ItemPathSpacing);

            for (int i = 1; i <= itemCount; i++)
            {
                float t = (float)i / (itemCount + 1);
                Vector3 spawnPos = Vector3.Lerp(startPoint, endPoint, t);
                CreateItemObject(spawnPos);
            }
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
