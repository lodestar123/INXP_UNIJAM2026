using System.Collections.Generic;
using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적을 가로 레인에 생성하고, 시간이 흐를수록 적 체력을 올림
    /// 또한 적 처치 시 아이템 드랍(1~3개, 3개가 모두 동일하지 않도록)을 담당
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

            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver))
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
            go.transform.position = new Vector3(x, _config.enemySpawnY, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = type.enemySprite;
            sr.color = Color.white;
            sr.sortingOrder = 3;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.42f;
            col.isTrigger = true;

            var enemy = go.AddComponent<GalagaEnemy>();
            enemy.Initialize(_config, _owner, type, hp, holdY, sr, _container);

            _aliveEnemies.Add(enemy);
        }

        private int CurrentEnemyHp()
        {
            int steps = Mathf.FloorToInt(_elapsed / Mathf.Max(0.01f, _config.hpIncreaseInterval));
            int hp = _config.enemyBaseHp + steps * Mathf.Max(0, _config.hpIncreasePerStep);
            return Mathf.Clamp(hp, 1, Mathf.Max(1, _config.maxEnemyHp));
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

        /// <summary>적이 완전히 제거될 때 스포너의 목록에서 제외합니다.</summary>
        public void UnregisterEnemy(GalagaEnemy enemy)
        {
            _aliveEnemies.Remove(enemy);
        }

        /// <summary>
        /// 적 처치 위치에서 1~3개의 아이템을 떨어뜨립니다. 3개일 때 모두 같은 종류가
        /// 되지 않도록 보정합니다.
        /// </summary>
        public void SpawnItemDrops(Vector3 position)
        {
            if (_config == null) return;

            int min = Mathf.Max(1, _config.minDropCount);
            int max = Mathf.Max(min, _config.maxDropCount);
            int count = Random.Range(min, max + 1);

            Item[] items = ItemDataBase.Items;
            if (items == null || items.Length == 0)
            {
                Debug.LogWarning("[Galaga] Stage 4 ItemDataBase가 비어 있어 드랍을 생성하지 않습니다.");
                return;
            }

            int[] chosen = new int[count];
            for (int i = 0; i < count; i++)
            {
                chosen[i] = Random.Range(0, items.Length);
            }

            // 모두 동일하면 하나를 다른 것으로 교체 (선택지가 2개 이상일 때만)
            if (count >= 2 && items.Length >= 2 && AllSame(chosen))
            {
                int replaceIndex = Random.Range(0, count);
                int newValue;
                do
                {
                    newValue = Random.Range(0, items.Length);
                } while (newValue == chosen[replaceIndex]);
                chosen[replaceIndex] = newValue;
            }

            float spacing = Mathf.Max(0.2f, _config.itemDropHorizontalSpacing);
            float startX = position.x - spacing * (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                // 동시에 화면 밖으로 나갈 때 같은 Item 참조가 쿨다운에 걸리지 않도록 Y를 살짝 분산
                float yOffset = i * 0.12f;
                Vector3 dropPos = new Vector3(startX + spacing * i, position.y - yOffset, 0f);
                Item item = items[chosen[i]];
                SpawnItemPickup(dropPos, item);
            }
        }

        private void SpawnItemPickup(Vector3 position, Item item)
        {
            var go = new GameObject("ItemPickup");
            if (_container != null) go.transform.SetParent(_container, true);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            if (item != null && item.spritePast != null)
            {
                sr.sprite = item.spritePast;
            }
            else if (_config.itemDropFallbackSprite != null)
            {
                sr.sprite = _config.itemDropFallbackSprite;
            }
            else
            {
                Debug.LogWarning("[Galaga] 아이템 스프라이트가 없어 드랍을 생성하지 않습니다.");
                Destroy(go);
                return;
            }
            sr.sortingOrder = 6;

            var pickup = go.AddComponent<GalagaItemPickup>();
            pickup.Initialize(_owner, item, _config.itemFallDuration, _config.bottomDespawnY, sr);
        }

        private static bool AllSame(int[] values)
        {
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] != values[0]) return false;
            }
            return true;
        }

        private void ClearEnemies()
        {
            for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
            {
                if (_aliveEnemies[i] != null)
                {
                    Destroy(_aliveEnemies[i].gameObject);
                }
            }
            _aliveEnemies.Clear();

            // 컨테이너에 남은 적/총알/아이템/레이저까지 모두 정리 (애니팡 전환 시 잔상 방지)
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
