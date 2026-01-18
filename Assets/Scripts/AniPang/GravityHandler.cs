using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 보드에 중력을 적용하는 클래스
/// </summary>
public class GravityHandler
{
    private readonly Tile[,] _tiles;
    private readonly int _width;
    private readonly int _height;
    private const float ShrinkDuration = 0.3f; // 작아지는 시간
    private const float GrowDuration = 0.3f; // 커지는 시간

    public GravityHandler(Tile[,] tiles)
    {
        _tiles = tiles;
        _width = tiles.GetLength(0);
        _height = tiles.GetLength(1);
    }

    /// <summary>
    /// 중력만 적용 (리필 없음) - 스케일 변화 연출 포함
    /// </summary>
    public async Task ApplyGravityOnly()
    {
        // 1단계: 이동해야 할 아이템들의 원래 위치 타일과 목적지 타일 수집
        var tilesToShrink = new List<Tile>(); // 작아지며 사라질 타일들
        var tilesToGrow = new HashSet<(int x, int y)>(); // 커지며 나타날 타일들 (이동한 아이템만)
        
        for (int x = 0; x < _width; x++)
        {
            // 위 -> 아래로 남아있는 아이템과 원래 위치 수집
            var itemData = new List<(Item item, int originalY)>();
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null || !t.button.interactable) continue;

                if (t.Item != null)
                {
                    itemData.Add((t.Item, y));
                }
            }

            // 목적지 위치 계산
            int targetIdx = 0;
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null) continue;
                if (!t.button.interactable) continue;

                if (targetIdx < itemData.Count)
                {
                    var (item, originalY) = itemData[targetIdx];
                    
                    // 위치가 다르면 이동 필요
                    if (originalY != y)
                    {
                        var originalTile = _tiles[x, originalY];
                        if (originalTile != null && !tilesToShrink.Contains(originalTile))
                        {
                            tilesToShrink.Add(originalTile);
                        }
                        // 이동한 아이템의 목적지 타일 기록
                        tilesToGrow.Add((x, y));
                    }
                    targetIdx++;
                }
            }
        }

        // 2단계: 원래 위치에서 작아지며 사라지는 애니메이션
        if (tilesToShrink.Count > 0)
        {
            var shrinkSequence = DOTween.Sequence();
            foreach (var tile in tilesToShrink)
            {
                if (tile.icon != null)
                {
                    shrinkSequence.Join(tile.icon.transform.DOScale(Vector3.zero, ShrinkDuration).SetEase(Ease.InBack));
                }
            }
            await shrinkSequence.Play().AsyncWaitForCompletion();
        }

        // 3단계: 실제 데이터 이동
        for (int x = 0; x < _width; x++)
        {
            var remain = new List<Item>();
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null || !t.button.interactable) continue;

                if (t.Item != null) remain.Add(t.Item);
            }

            int idx = 0;
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null) continue;
                if (!t.button.interactable) continue;

                if (idx < remain.Count)
                {
                    TileItemSetter.SetTileItem(t, remain[idx]);
                    // 이동한 아이템만 스케일 0으로 시작
                    if (tilesToGrow.Contains((x, y)) && t.icon != null)
                    {
                        t.icon.transform.localScale = Vector3.zero;
                    }
                    idx++;
                }
                else
                {
                    TileItemSetter.SetTileItem(t, null);
                }
            }
        }

        // 4단계: 이동한 아이템들만 커지며 나타나는 애니메이션
        if (tilesToGrow.Count > 0)
        {
            var growSequence = DOTween.Sequence();
            foreach (var (x, y) in tilesToGrow)
            {
                var t = _tiles[x, y];
                if (t != null && t.Item != null && t.icon != null)
                {
                    growSequence.Join(t.icon.transform.DOScale(Vector3.one, GrowDuration).SetEase(Ease.OutBack));
                }
            }
            
            if (growSequence.Duration() > 0)
            {
                await growSequence.Play().AsyncWaitForCompletion();
            }
        }

        // 중력 적용 후 정리: 상태 일관성 보장
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var t = _tiles[x, y];
                if (t == null) continue;

                if (!t.button.interactable)
                {
                    if (t.Item != null)
                    {
                        t.Item = null;
                        t.icon.gameObject.SetActive(false);
                    }
                }
                else if (t.Item == null)
                {
                    t.button.interactable = false;
                    t.icon.gameObject.SetActive(false);
                }
                else
                {
                    // 아이템이 있는 타일은 스케일 복원 보장
                    if (t.icon != null)
                    {
                        t.icon.transform.localScale = Vector3.one;
                    }
                }
            }
        }
    }
}
