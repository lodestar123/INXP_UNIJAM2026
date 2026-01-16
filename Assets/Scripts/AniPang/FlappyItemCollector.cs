using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플래피버드 게임에서 수집한 아이템을 저장하고 관리하는 클래스
/// </summary>
public static class FlappyItemCollector
{
    private static readonly List<Item> _collectedItems = new List<Item>();

    //아이템을 수집
    public static void CollectItem(Item item)
    {
        if (item != null)
        {
            _collectedItems.Add(item);
            Debug.Log($"Item collected: {item.name}, Total: {_collectedItems.Count}");
        }
    }

    // 수집한 모든 아이템 리스트를 반환
    public static List<Item> GetCollectedItems()
    {
        return new List<Item>(_collectedItems);
    }

    // 수집한 아이템 개수 반환
    public static int GetItemCount()
    {
        return _collectedItems.Count;
    }

    // 수집한 아이템을 초기화 (새 게임 시작 시)
    public static void ClearItems()
    {
        _collectedItems.Clear();
        Debug.Log("Collected items cleared");
    }

    // 특정 아이템 타입의 개수를 반환
    public static int GetItemCountByType(Item item)
    {
        int count = 0;
        foreach (var collectedItem in _collectedItems)
        {
            if (collectedItem == item)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 테스트용: 아이템 리스트를 직접 설정합니다.
    /// </summary>
    /// <param name="items">설정할 아이템 리스트</param>
    public static void SetTestItems(List<Item> items)
    {
        _collectedItems.Clear();
        if (items != null)
        {
            _collectedItems.AddRange(items);
            Debug.Log($"[테스트] 아이템 {_collectedItems.Count}개 설정됨");
        }
    }

    /// <summary>
    /// 테스트용: 특정 아이템을 여러 개 추가합니다.
    /// </summary>
    /// <param name="item">추가할 아이템</param>
    /// <param name="count">추가할 개수</param>
    public static void AddTestItems(Item item, int count)
    {
        if (item == null) return;
        
        for (int i = 0; i < count; i++)
        {
            _collectedItems.Add(item);
        }
        Debug.Log($"[테스트] {item.name} {count}개 추가됨 (총 {_collectedItems.Count}개)");
    }
}
