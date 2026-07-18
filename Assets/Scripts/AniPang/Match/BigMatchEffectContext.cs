using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 4개 이상 한 번에 pop 될 때 이펙트 재생에 필요한 정보
/// </summary>
public readonly struct BigMatchEffectContext
{
    public int MatchedCount { get; }
    public IReadOnlyCollection<Tile> MatchedTiles { get; }
    public Vector3 WorldCenter { get; }
    public Item PrimaryItem { get; }

    public BigMatchEffectContext(
        int matchedCount,
        IReadOnlyCollection<Tile> matchedTiles,
        Vector3 worldCenter,
        Item primaryItem)
    {
        MatchedCount = matchedCount;
        MatchedTiles = matchedTiles;
        WorldCenter = worldCenter;
        PrimaryItem = primaryItem;
    }
}
