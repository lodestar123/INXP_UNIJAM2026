using FlappyBird.Configs;
using FlappyBird.Components;
using UnityEngine;
using Utils;

namespace FlappyBird
{
    // 파이프와 아이템의 주기적인 생성을 담당하는 클래스입니다.
    // 설정값(Config)을 읽어 적절한 컴포넌트를 조립하여 오브젝트를 소환합니다.
    public class PipeSpawner : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private FlappyBirdConfig config;

        [Header("프리팹")]
        [SerializeField] private GameObject pipePrefab;

        private bool _isSpawning = false;
        private float _timer = 0f;
        private float _lastSpawnY; 

        // 태그 상수 정의
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

        // 파이프 스폰 프로세스를 시작합니다.
        public void StartSpawning()
        {
            _isSpawning = true;
            _timer = 0f;
            _lastSpawnY = (config.PipeMinY + config.PipeMaxY) / 2f;
        }

        // 파이프 스폰 프로세스를 중지합니다.
        public void StopSpawning()
        {
            _isSpawning = false;
        }

        // 화면에 존재하는 모든 파이프/아이템을 정리합니다.
        public void ClearPipes()
        {
            BoundaryRecycler[] activeObjects = FindObjectsByType<BoundaryRecycler>(FindObjectsSortMode.None);
            
            foreach (BoundaryRecycler obj in activeObjects)
            {
                obj.transform.localScale = Vector3.one;
                obj.gameObject.SetActive(false); 
            }
        }

        // 설정에 따라 장애물 패턴(일반 통로 또는 갈림길)을 생성합니다.
        private void SpawnObstaclePattern()
        {
            float nextY = CalculateNextSpawnHeight();
            _lastSpawnY = nextY;

            bool shouldSpawnItem = Random.value < config.ItemSpawnChance;

            if (Random.value < config.DoublePipeChance)
            {
                CreateBranchingPipes(nextY, shouldSpawnItem);
            }
            else
            {
                CreateStandardPipePair(nextY, shouldSpawnItem);
            }
        }

        private float CalculateNextSpawnHeight()
        {
            float variance = Random.Range(-config.PipeHeightVariance, config.PipeHeightVariance);
            float newY = _lastSpawnY + variance;
            return Mathf.Clamp(newY, config.PipeMinY, config.PipeMaxY);
        }

        // 일반 패턴: 위/아래 파이프 생성 + 아이템
        private void CreateStandardPipePair(float centerY, bool spawnItem)
        {
            float halfGap = config.GapHeight / 2f;

            CreatePipeObject(centerY - halfGap, false, config.PipeSize);
            CreatePipeObject(centerY + halfGap, true, config.PipeSize);

            if (spawnItem)
            {
                Vector3 itemPos = new Vector3(config.PipeSpawnX, centerY, 0);
                CreateItemObject(itemPos);
            }
        }

        // 갈림길 패턴: 중앙 장애물 + 위/아래 통로 생성
        private void CreateBranchingPipes(float centerY, bool spawnItem)
        {
            CreatePipeObject(centerY, false, config.InnerPipeSize); 

            float spacing = config.DoublePipeVerticalSpacing;

            CreatePipeObject(centerY - spacing, false, config.PipeSize);
            CreatePipeObject(centerY + spacing, true, config.PipeSize);

            if (spawnItem)
            {
                bool isTopPath = Random.value > 0.5f;
                float offset = spacing / 2f;
                float itemY = isTopPath ? (centerY + offset) : (centerY - offset);
                
                Vector3 itemPos = new Vector3(config.PipeSpawnX, itemY, 0);
                CreateItemObject(itemPos);
            }
        }

        // 파이프 오브젝트를 생성하고 물리/이동 컴포넌트를 설정합니다.
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

        // 아이템 오브젝트를 생성하고 물리/이동 컴포넌트를 설정합니다.
        private void CreateItemObject(Vector3 position)
        {
            if (config.ItemPrefab == null) return;

            GameObject itemInstance = ObjectPool.Instance.Spawn(config.ItemPrefab, position, Quaternion.identity);
            itemInstance.tag = TAG_ITEM;

            AttachComponents(itemInstance, config.ItemPrefab);
        }

        // 생성된 오브젝트에 이동 및 재활용 로직을 부착합니다.
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
