using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보드에서 매칭된 타일들을 감지하는 클래스
/// </summary>
public class MatchDetector
{
    private readonly Tile[,] _tiles;
    private readonly int _width;
    private readonly int _height;

    public MatchDetector(Tile[,] tiles)
    {
        _tiles = tiles;
        _width = tiles.GetLength(0);
        _height = tiles.GetLength(1);
    }

    /// <summary>
    /// 보드에 팝 가능한 매칭이 있는지 확인
    /// </summary>
    public bool CanPop()
    {
        // 각 행을 확인
        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width - 2; x++)
            {
                if (AreConsecutiveTiles(_tiles[x, y], _tiles[x + 1, y], _tiles[x + 2, y]))
                    return true;
            }
        }

        // 각 열을 확인
        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height - 2; y++)
            {
                if (AreConsecutiveTiles(_tiles[x, y], _tiles[x, y + 1], _tiles[x, y + 2]))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 보드에 연속된 버블이 있는지 확인 (초기 보드 생성 시 사용)
    /// </summary>
    public bool HasConsecutiveBubbles()
    {
        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width - 2; x++)
            {
                if (AreConsecutiveTiles(_tiles[x, y], _tiles[x + 1, y], _tiles[x + 2, y]))
                    return true;
            }
        }

        for (var x = 0; x < _width; x++)
        {
            for (var y = 0; y < _height - 2; y++)
            {
                if (AreConsecutiveTiles(_tiles[x, y], _tiles[x, y + 1], _tiles[x, y + 2]))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 특정 위치에 연속된 타일이 있는지 확인 (보드 생성 시 사용)
    /// </summary>
    public bool IsConsecutiveTile(int x, int y, Item item)
    {
        // 현재 타일 주변의 타일들이 같은 아이템인지 확인
        return (x > 1 && _tiles[x - 1, y].Item == item && _tiles[x - 2, y].Item == item) ||
               (y > 1 && _tiles[x, y - 1].Item == item && _tiles[x, y - 2].Item == item);
    }

    /// <summary>
    /// 세 개의 타일이 연속된 같은 아이템인지 확인
    /// </summary>
    public bool AreConsecutiveTiles(Tile tile1, Tile tile2, Tile tile3)
    {
        // 빈 타일이 있으면 false 반환
        if (tile1 == null || tile2 == null || tile3 == null ||
            tile1.Item == null || tile2.Item == null || tile3.Item == null)
        {
            return false;
        }
        
        // 타일이 서로 같은 종류이면서 연속되어 있는지 확인
        return (tile1.Item == tile2.Item && tile2.Item == tile3.Item);
    }

    /// <summary>
    /// 보드에서 매칭된 모든 타일들을 반환
    /// </summary>
    public HashSet<Tile> GetAllMatchedTiles()
    {
        var matched = new HashSet<Tile>();

        // 가로 체크
        for (int y = 0; y < _height; y++)
        {
            int run = 1;
            for (int x = 1; x < _width; x++)
            {
                var prev = _tiles[x - 1, y];
                var cur = _tiles[x, y];

                bool same =
                    prev != null && cur != null &&
                    prev.button.interactable && cur.button.interactable &&
                    prev.Item != null && cur.Item != null &&
                    prev.Item == cur.Item;

                if (same) run++;
                else
                {
                    if (run >= 3)
                    {
                        for (int k = 0; k < run; k++)
                            matched.Add(_tiles[x - 1 - k, y]);
                    }
                    run = 1;
                }
            }

            if (run >= 3)
            {
                for (int k = 0; k < run; k++)
                    matched.Add(_tiles[_width - 1 - k, y]);
            }
        }

        // 세로 체크
        for (int x = 0; x < _width; x++)
        {
            int run = 1;
            for (int y = 1; y < _height; y++)
            {
                var prev = _tiles[x, y - 1];
                var cur = _tiles[x, y];

                bool same =
                    prev != null && cur != null &&
                    prev.button.interactable && cur.button.interactable &&
                    prev.Item != null && cur.Item != null &&
                    prev.Item == cur.Item;

                if (same) run++;
                else
                {
                    if (run >= 3)
                    {
                        for (int k = 0; k < run; k++)
                            matched.Add(_tiles[x, y - 1 - k]);
                    }
                    run = 1;
                }
            }

            if (run >= 3)
            {
                for (int k = 0; k < run; k++)
                    matched.Add(_tiles[x, _height - 1 - k]);
            }
        }

        return matched;
    }
}
