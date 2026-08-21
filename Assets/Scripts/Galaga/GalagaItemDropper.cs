using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적 처치 시 아이템을 떨어뜨림 (3~5개, 모두 같은 종류가 되지 않도록 보정)
    /// </summary>
    public class GalagaItemDropper : MonoBehaviour
    {
        private GalagaConfig _config;
        private GalagaGameManager _owner;
        private Transform _container;

        public void Initialize(GalagaConfig config, GalagaGameManager owner, Transform container = null)
        {
            _config = config;
            _owner = owner;
            _container = container;
        }

        // 적 처치 위치에서 3~5개의 아이템을 떨어뜨림
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
    }
}
