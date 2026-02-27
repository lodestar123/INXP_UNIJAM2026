using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 아이템 큐 시스템 - 순서 보존 및 append 지원
/// 아이템 리스트를 순서대로 저장하고 관리합니다.
/// </summary>
[Serializable]
public class ItemQueue
{
    [SerializeField] private List<string> _itemNames = new List<string>(); // Item의 name을 저장 (ScriptableObject 참조 유지)
    
    // 임시로 Item 리스트를 저장 (직렬화 불가능하므로 name으로 저장)
    private List<Item> _items = new List<Item>();

    /// <summary>
    /// 큐에 저장된 아이템 개수
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// 큐가 비어있는지 확인
    /// </summary>
    public bool IsEmpty => _items.Count == 0;

    /// <summary>
    /// 읽기 전용 아이템 리스트 반환 (순서 보존)
    /// </summary>
    public IReadOnlyList<Item> Items => _items.AsReadOnly();

    /// <summary>
    /// 아이템을 큐의 끝에 추가 (append)
    /// </summary>
    public void Enqueue(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemQueue] null 아이템은 추가할 수 없습니다.");
            return;
        }

        _items.Add(item);
        _itemNames.Add(item.name);
        //Debug.Log($"[ItemQueue] 아이템 추가: {item.name}, 총 개수: {_items.Count}");
    }

    /// <summary>
    /// 여러 아이템을 한 번에 큐의 끝에 추가 (append)
    /// </summary>
    public void EnqueueRange(IEnumerable<Item> items)
    {
        if (items == null) return;

        int addedCount = 0;
        foreach (var item in items)
        {
            if (item != null)
            {
                _items.Add(item);
                _itemNames.Add(item.name);
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            Debug.Log($"[ItemQueue] {addedCount}개 아이템 추가됨, 총 개수: {_items.Count}");
        }
    }

    /// <summary>
    /// 큐의 앞에서부터 지정된 개수만큼 아이템을 가져옴 (제거하지 않음)
    /// </summary>
    public List<Item> Peek(int count)
    {
        if (count <= 0) return new List<Item>();
        if (count > _items.Count) count = _items.Count;

        return _items.Take(count).ToList();
    }

    /// <summary>
    /// 큐의 앞에서부터 지정된 개수만큼 아이템을 제거하고 반환
    /// </summary>
    public List<Item> Dequeue(int count)
    {
        if (count <= 0) return new List<Item>();
        if (count > _items.Count) count = _items.Count;

        var result = _items.Take(count).ToList();
        _items.RemoveRange(0, count);
        _itemNames.RemoveRange(0, count);

        Debug.Log($"[ItemQueue] {count}개 아이템 제거됨, 남은 개수: {_items.Count}");
        return result;
    }

    /// <summary>
    /// 큐의 첫 번째 아이템을 제거하고 반환
    /// </summary>
    public Item Dequeue()
    {
        if (_items.Count == 0) return null;

        var item = _items[0];
        _items.RemoveAt(0);
        _itemNames.RemoveAt(0);

        Debug.Log($"[ItemQueue] 아이템 제거: {item.name}, 남은 개수: {_items.Count}");
        return item;
    }

    /// <summary>
    /// 큐의 모든 아이템을 제거
    /// </summary>
    public void Clear()
    {
        int count = _items.Count;
        _items.Clear();
        _itemNames.Clear();
        Debug.Log($"[ItemQueue] 모든 아이템 제거됨 (기존 개수: {count})");
    }

    /// <summary>
    /// 큐의 모든 아이템을 리스트로 반환 (복사본)
    /// </summary>
    public List<Item> GetAllItems()
    {
        return new List<Item>(_items);
    }

    /// <summary>
    /// 직렬화를 위해 Item 리스트를 name 리스트로 변환하여 저장
    /// </summary>
    public void Serialize()
    {
        _itemNames.Clear();
        foreach (var item in _items)
        {
            if (item != null)
            {
                _itemNames.Add(item.name);
            }
        }
    }

    /// <summary>
    /// 직렬화된 name 리스트를 Item 리스트로 복원
    /// ItemDataBase를 통해 Item을 찾아서 복원합니다.
    /// </summary>
    public void Deserialize()
    {
        _items.Clear();
        
        if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0)
        {
            Debug.LogError("[ItemQueue] ItemDataBase.Items가 비어있습니다. 아이템을 복원할 수 없습니다.");
            return;
        }

        // ItemDataBase의 Items 배열에서 name으로 찾기
        var itemDict = new Dictionary<string, Item>();
        foreach (var item in ItemDataBase.Items)
        {
            if (item != null && !itemDict.ContainsKey(item.name))
            {
                itemDict[item.name] = item;
            }
        }

        foreach (var itemName in _itemNames)
        {
            if (itemDict.TryGetValue(itemName, out Item item))
            {
                _items.Add(item);
            }
            else
            {
                Debug.LogWarning($"[ItemQueue] 아이템 '{itemName}'을 찾을 수 없습니다.");
            }
        }

        Debug.Log($"[ItemQueue] {_items.Count}개 아이템 복원됨");
    }

    /// <summary>
    /// 디버그용: 현재 큐 상태 출력
    /// </summary>
    public void DebugPrint()
    {
        Debug.Log($"[ItemQueue] 총 {_items.Count}개 아이템:");
        for (int i = 0; i < _items.Count; i++)
        {
            Debug.Log($"  [{i}] {_items[i]?.name ?? "null"}");
        }
    }
}
