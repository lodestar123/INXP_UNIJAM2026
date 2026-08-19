using System.Collections.Generic;
using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적을 가로 레인에 생성하고, 시간이 흐를수록 적 공격 속도를 올림
    /// </summary>
    public class GalagaEnemySpawner : MonoBehaviour
    {
        private GalagaConfig _config;
        private GalagaGameManager _owner;
        private Transform _container;

        private readonly List<GalagaEnemy> _aliveEnemies = new List<GalagaEnemy>();
        private float _elapsed;
        private float _spawnTimer;
        private bool _running;

        public float ElapsedTime => _elapsed;

        public void Initialize(GalagaConfig config, GalagaGameManager owner, Transform container = null)
        {
            _config = config;
            _owner = owner;
            _container = container;
        }

        public void StartSpawning()
        {
            _elapsed = 0f;
            _spawnTimer = -Mathf.Max(0f, _config.initialSpawnDelay);
            _running = true;
        }

        public void StopSpawning()
        {
            _running = false;
            ClearEnemies();
        }

        public void ResetState()
        {
            _running = false;
            _elapsed = 0f;
            _spawnTimer = 0f;
            ClearEnemies();
        }

        private void Update()
        {
            if (!_running || _config == null) return;

            if (GalagaGameManager.IsGameplayFrozen)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer < _config.enemySpawnInterval) return;
            if (_aliveEnemies.Count >= _config.maxAliveEnemies) return;

            _spawnTimer = 0f;
            SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            GalagaEnemyType type = PickRandomTypeWithSprite();
            if (type == null) return;
            int hp = CurrentEnemyHp();
            float x = PickLaneX();
            float holdY = Random.Range(
                Mathf.Min(_config.enemyHoldYMin, _config.enemyHoldYMax),
                Mathf.Max(_config.enemyHoldYMin, _config.enemyHoldYMax));

            var go = new GameObject("Enemy");
            if (_container != null) go.transform.SetParent(_container, true);
            go.transform.position = new Vector3(x, holdY + Mathf.Max(0.6f, _config.enemyEntryDropHeight), 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = type.enemySprite;
            sr.color = Color.white;
            sr.sortingOrder = 3;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.42f;
            col.isTrigger = true;

            var enemy = go.AddComponent<GalagaEnemy>();
            enemy.Initialize(_config, _owner, type, hp, holdY, sr);

            _aliveEnemies.Add(enemy);
        }

        private int CurrentEnemyHp()
        {
            return Mathf.Max(1, _config.enemyBaseHp);
        }

        private GalagaEnemyType PickRandomTypeWithSprite()
        {
            if (_config.enemyTypes == null || _config.enemyTypes.Count == 0)
            {
                Debug.LogWarning("[Galaga] GalagaConfig.enemyTypes가 비어 있습니다.");
                return null;
            }

            int attempts = _config.enemyTypes.Count * 2;
            for (int i = 0; i < attempts; i++)
            {
                GalagaEnemyType type = _config.enemyTypes[Random.Range(0, _config.enemyTypes.Count)];
                if (type != null && type.enemySprite != null)
                {
                    return type;
                }
            }

            Debug.LogWarning("[Galaga] enemyTypes에 enemySprite가 할당된 항목이 없습니다.");
            return null;
        }

        private float PickLaneX()
        {
            GalagaPlayArea.GetHorizontalBounds(_config, Camera.main, out float min, out float max);

            int laneCount = Mathf.Max(1, _config.laneCount);
            float width = max - min;
            float laneWidth = width / laneCount;

            // 현재 살아있는 적이 차지한 레인을 피해서 배치 시도
            var occupied = new HashSet<int>();
            for (int i = 0; i < _aliveEnemies.Count; i++)
            {
                if (_aliveEnemies[i] == null) continue;
                float ex = _aliveEnemies[i].transform.position.x;
                int lane = Mathf.Clamp(Mathf.FloorToInt((ex - min) / laneWidth), 0, laneCount - 1);
                occupied.Add(lane);
            }

            int chosenLane;
            if (occupied.Count >= laneCount)
            {
                chosenLane = Random.Range(0, laneCount);
            }
            else
            {
                do
                {
                    chosenLane = Random.Range(0, laneCount);
                } while (occupied.Contains(chosenLane));
            }

            float laneCenter = min + laneWidth * (chosenLane + 0.5f);
            return laneCenter;
        }

        // 적이 완전히 제거될 때 스포너 목록에서 제외
        public void UnregisterEnemy(GalagaEnemy enemy)
        {
            _aliveEnemies.Remove(enemy);
        }

        private void ClearEnemies()
        {
            _owner?.ProjectilePool?.CollectAll();

            for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (_aliveEnemies[i] != null)
                {
                    Destroy(_aliveEnemies[i].gameObject);
                }
            }
            _aliveEnemies.Clear();

            // 컨테이너에 남은 아이템 등 잔여 오브젝트 정리 (애니팡 전환 시 잔상 방지)
            if (_container != null)
            {
                for (int i = _container.childCount - 1; i >= 0; i--)
                {
                    Destroy(_container.GetChild(i).gameObject);
                }
            }
        }
    }
}
