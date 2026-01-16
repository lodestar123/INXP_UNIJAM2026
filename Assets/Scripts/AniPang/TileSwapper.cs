using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 타일 스왑 및 선택 로직을 처리하는 클래스
/// </summary>
public class TileSwapper
{
    private readonly Tile[,] _tiles;
    private readonly List<Tile> _selection = new List<Tile>();
    private readonly MatchDetector _matchDetector;
    private readonly PopHandler _popHandler;
    private const float TweenDuration = 0.25f;

    public TileSwapper(Tile[,] tiles, MatchDetector matchDetector, PopHandler popHandler)
    {
        _tiles = tiles;
        _matchDetector = matchDetector;
        _popHandler = popHandler;
    }

    /// <summary>
    /// 타일 선택 및 스왑 처리
    /// </summary>
    public async void Select(Tile tile)
    {
        // 빈 타일은 선택 불가능
        if (tile == null || tile.Item == null)
        {
            return;
        }
        
        if (!_selection.Contains(tile))
        {
            if (_selection.Count > 0)
            {
                if (Array.IndexOf(_selection[0].Neighbours, tile) != -1)
                {
                    _selection.Add(tile);
                }
            }
            else
            {
                _selection.Add(tile);
            }
        }

        if (_selection.Count < 2) return;

        Debug.Log($"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]);

        if (_matchDetector.CanPop())
        {
            while (_matchDetector.CanPop()) await _popHandler.Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }

    /// <summary>
    /// 두 타일을 스왑합니다
    /// </summary>
    public async Task Swap(Tile tile1, Tile tile2)
    {
        // 안전장치: null / 비활성 타일 / 빈 타일은 스왑 금지
        if (tile1 == null || tile2 == null) return;
        if (!tile1.button.interactable || !tile2.button.interactable) return;
        if (tile1.Item == null || tile2.Item == null) return;

        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var t1 = icon1.transform;
        var t2 = icon2.transform;

        Vector3 p1 = t1.position;
        Vector3 p2 = t2.position;

        // 스왑 전 아이템(=sprite 소스)
        Item item1 = tile1.Item;
        Item item2 = tile2.Item;

        // 1) "움직이는 것처럼" 보이게만 연출 (Transform은 끝나면 원복)
        var seq = DOTween.Sequence();
        seq.Join(t1.DOMove(p2, TweenDuration));
        seq.Join(t2.DOMove(p1, TweenDuration));

        await seq.Play().AsyncWaitForCompletion();

        // 2) Transform 원복 (오브젝트는 제자리 유지)
        t1.position = p1;
        t2.position = p2;

        // 3) 실제 스왑은 데이터/이미지만 교체 (오브젝트/레퍼런스 변경 X)
        TileItemSetter.SetTileItem(tile1, item2);
        TileItemSetter.SetTileItem(tile2, item1);
    }
}
