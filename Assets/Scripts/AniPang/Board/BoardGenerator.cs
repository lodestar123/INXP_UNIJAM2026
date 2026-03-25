using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 애니팡 게임 보드 생성
/// </summary>
public static class BoardGenerator
{
    /// <summary>
    /// 보드 분석 결과를 담는 클래스
    /// </summary>
    public class BoardAnalysis
    {
        public List<Item> LackingItems = new List<Item>();  // 부족 블럭
        public List<Item> NormalItems = new List<Item>();    // 일반 블럭
        public List<Item> ExcessItems = new List<Item>();    // 과잉 블럭
    }

    /// <summary>
    /// 플래피버드에서 수집한 아이템을 기반으로 보드 아이템 시퀀스를 생성합니다.
    /// </summary>
    /// <param name="collectedItems">플래피버드에서 수집한 아이템 리스트</param>
    /// <param name="width">보드 너비</param>
    /// <param name="height">보드 높이</param>
    /// <returns>보드에 배치할 아이템 시퀀스 (왼쪽 아래부터 오른쪽 위까지 순서)</returns>
    public static List<Item> GenerateBoardSequence(List<Item> collectedItems, int width, int height)
    {
        var itemSequence = new List<Item>();
        int totalTiles = width * height;

        // 보드 분석: 부족/일반/과잉 블럭 분류
        var boardAnalysis = AnalyzeBoard(collectedItems);

        // 주력 블럭 결정 (부족 블럭 우선)
        Item primaryItem = null;
        if (boardAnalysis.LackingItems.Count > 0)
        {
            primaryItem = boardAnalysis.LackingItems[Random.Range(0, boardAnalysis.LackingItems.Count)];
        }
        else if (ItemDataBase.Items.Length > 0)
        {
            primaryItem = ItemDataBase.Items[Random.Range(0, ItemDataBase.Items.Length)];
        }

        // 최소 보장: 각 블럭 최소 3개 이상 (순차 배치 시 연속 제한 고려)
        var minGuarantee = EnsureMinimumItems(collectedItems, totalTiles);
        foreach (var item in minGuarantee)
        {
            if (itemSequence.Count >= totalTiles) break;

            // 연속 제한 체크
            var excluded = GetExcludedItems(itemSequence, width);
            if (excluded.Contains(item))
            {
                // 대체 아이템 선택
                Item alternative = SelectItemWithWeight(boardAnalysis, itemSequence, primaryItem, width);
                itemSequence.Add(alternative);
            }
            else
            {
                itemSequence.Add(item);
            }
        }

        // 나머지 타일 채우기
        while (itemSequence.Count < totalTiles)
        {
            Item selectedItem = SelectItemWithWeight(boardAnalysis, itemSequence, primaryItem, width);
            itemSequence.Add(selectedItem);
        }

        return itemSequence;
    }

    /// <summary>
    /// 보드 분석: 부족/일반/과잉 블럭 분류
    /// </summary>
    private static BoardAnalysis AnalyzeBoard(List<Item> collectedItems)
    {
        var analysis = new BoardAnalysis();

        // 수집한 아이템별 개수 계산
        var itemCounts = new Dictionary<Item, int>();
        foreach (var item in collectedItems)
        {
            if (!itemCounts.ContainsKey(item))
                itemCounts[item] = 0;
            itemCounts[item]++;
        }

        // 평균 개수 계산
        if (itemCounts.Count == 0)
        {
            return analysis;
        }

        float average = (float)collectedItems.Count / itemCounts.Count;

        // 부족/일반/과잉 분류
        foreach (var kvp in itemCounts)
        {
            if (kvp.Value < average)
                analysis.LackingItems.Add(kvp.Key);
            else if (kvp.Value > average)
                analysis.ExcessItems.Add(kvp.Key);
            else
                analysis.NormalItems.Add(kvp.Key);
        }

        return analysis;
    }

    /// <summary>
    /// 가중치 기반 아이템 선택 (주력 70%, 부족 15%, 일반 10%, 과잉 5%)
    /// </summary>
    private static Item SelectItemWithWeight(BoardAnalysis analysis, List<Item> currentSequence, Item primaryItem, int width)
    {
        // 제외할 아이템 목록 (3연속 방지)
        var excludedItems = GetExcludedItems(currentSequence, width);

        // 가중치 딕셔너리 생성
        var weights = new Dictionary<Item, float>();
        var availableItems = new List<Item>();

        // 주력 블럭 설정 (부족 블럭 우선)
        if (primaryItem == null && analysis.LackingItems.Count > 0)
        {
            primaryItem = analysis.LackingItems[Random.Range(0, analysis.LackingItems.Count)];
        }
        else if (primaryItem == null && ItemDataBase.Items.Length > 0)
        {
            primaryItem = ItemDataBase.Items[Random.Range(0, ItemDataBase.Items.Length)];
        }

        // 모든 아이템에 가중치 부여
        foreach (var item in ItemDataBase.Items)
        {
            if (excludedItems.Contains(item))
                continue;

            availableItems.Add(item);

            if (item == primaryItem)
                weights[item] = 0.7f;
            else if (analysis.LackingItems.Contains(item))
                weights[item] = 0.15f;
            else if (analysis.NormalItems.Contains(item))
                weights[item] = 0.1f;
            else if (analysis.ExcessItems.Contains(item))
                weights[item] = 0.05f;
            else
                weights[item] = 0.1f; // 기본값
        }

        // 가중치 재분배 (제외된 아이템의 가중치를 나머지에 분배)
        if (availableItems.Count == 0)
        {
            // 모든 아이템이 제외된 경우, 제외 해제하고 균등 분배
            availableItems.AddRange(ItemDataBase.Items);
            float equalWeight = 1.0f / availableItems.Count;
            foreach (var item in availableItems)
            {
                weights[item] = equalWeight;
            }
        }
        else
        {
            // 가중치 정규화
            float totalWeight = 0f;
            foreach (var item in availableItems)
            {
                totalWeight += weights[item];
            }

            // 정규화
            if (totalWeight > 0f)
            {
                foreach (var item in availableItems)
                {
                    weights[item] /= totalWeight;
                }
            }
        }

        // 랜덤 선택
        float randomValue = Random.Range(0f, 1f);
        float cumulativeWeight = 0f;

        foreach (var item in availableItems)
        {
            cumulativeWeight += weights[item];
            if (randomValue <= cumulativeWeight)
            {
                return item;
            }
        }

        // 기본값 반환
        return availableItems.Count > 0 ? availableItems[0] : ItemDataBase.Items[0];
    }

    /// <summary>
    /// 현재 시퀀스에서 제외해야 할 아이템 목록 (3연속 방지)
    /// N번째 아이템 생성 시: 가로(N-1, N-2), 세로(N-7, N-14) 체크
    /// </summary>
    private static HashSet<Item> GetExcludedItems(List<Item> sequence, int width)
    {
        var excluded = new HashSet<Item>();

        if (sequence.Count < 2)
            return excluded;

        int n = sequence.Count;

        // 가로 연속 체크: N-1, N-2번 아이템
        if (n >= 2)
        {
            int index1 = n - 1;
            int index2 = n - 2;

            // 같은 행에 있는지 확인 (n % width로 같은 행 판단)
            int row1 = index1 / width;
            int row2 = index2 / width;

            if (row1 == row2) // 같은 행
            {
                Item item1 = sequence[index1];
                Item item2 = sequence[index2];

                if (item1 == item2) // 둘 다 같은 블럭이면 제외
                {
                    excluded.Add(item1);
                }
            }
        }

        // 세로 연속 체크: N-7, N-14번 아이템
        if (n >= 7)
        {
            int index7 = n - 7;
            Item item7 = sequence[index7];

            // 같은 열에 있는지 확인
            int colN = (n - 1) % width;
            int col7 = index7 % width;

            if (colN == col7) // 같은 열
            {
                if (n >= 14)
                {
                    int index14 = n - 14;
                    Item item14 = sequence[index14];
                    int col14 = index14 % width;

                    if (colN == col14 && item7 == item14) // 둘 다 같은 블럭이면 제외
                    {
                        excluded.Add(item7);
                    }
                }
            }
        }

        return excluded;
    }

    /// <summary>
    /// 최소 보장: 각 블럭 최소 3개 이상 배치
    /// </summary>
    private static List<Item> EnsureMinimumItems(List<Item> collectedItems, int totalTiles)
    {
        var result = new List<Item>();
        var itemCounts = new Dictionary<Item, int>();

        // 수집한 아이템별 개수 계산
        foreach (var item in collectedItems)
        {
            if (!itemCounts.ContainsKey(item))
                itemCounts[item] = 0;
            itemCounts[item]++;
        }

        // 각 아이템 최소 3개씩 배치
        const int minCount = 3;
        foreach (var kvp in itemCounts)
        {
            int needed = minCount;
            for (int i = 0; i < needed && result.Count < totalTiles; i++)
            {
                result.Add(kvp.Key);
            }
        }

        return result;
    }
}
