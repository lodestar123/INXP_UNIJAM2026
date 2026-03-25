using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class TileSwapper
{
    private readonly Tile[,] _tiles;
    private readonly List<Tile> _selection = new List<Tile>();
    private readonly MatchDetector _matchDetector;
    private readonly PopHandler _popHandler;
    private readonly Board _board;
    private const float TweenDuration = 0.25f;

    public TileSwapper(Tile[,] tiles, MatchDetector matchDetector, PopHandler popHandler, Board board = null)
    {
        _tiles = tiles;
        _matchDetector = matchDetector;
        _popHandler = popHandler;
        _board = board;
    }

    // 타일 선택 및 스왑 처리
    public async void Select(Tile tile)
    {
        // 빈 타일은 선택 불가능
        if (tile == null || tile.Item == null)
        {
            return;
        }
        
        // 이미 선택된 타일이 있는 경우
        if (_selection.Count > 0)
        {
            // 같은 타일을 다시 누른 경우 - 선택 취소
            if (_selection[0] == tile)
            {
                _selection.Clear();
                return;
            }
            
            // 주변 타일인 경우 - 두 번째 타일
            if (Array.IndexOf(_selection[0].Neighbours, tile) != -1)
            {
                _selection.Add(tile);
            }
            else
            {
                // 멀리 있는 타일인 경우 - 새로운 기준 타일
                _selection.Clear();
                _selection.Add(tile);
                return;
            }
        }
        else
        {
            // 첫 번째 타일 선택
            _selection.Add(tile);
            return;
        }

        // 두 번째 타일이 선택되었을 때만 스왑 진행
        if (_selection.Count < 2) return;

        if (_board != null)
        {
            _board.SetProcessing(true);
        }

        try
        {
            await Swap(_selection[0], _selection[1]);

            if (_matchDetector.CanPop())
            {
                while (_matchDetector.CanPop())
                {
                    await _popHandler.Pop();
                }
            }
            else
            {
                await Swap(_selection[0], _selection[1]);
            }
        }
        finally
        {
            if (_board != null)
            {
                _board.SetProcessing(false);
            }
        }

        _selection.Clear();
    }

    public async void SwapTiles(Tile tile1, Tile tile2)
    {
        if (tile1 == null || tile2 == null) return;
        if (tile1.Item == null || tile2.Item == null) return;
        if (!tile1.button.interactable || !tile2.button.interactable) return;
        
        _selection.Clear();

        if (_board != null)
        {
            _board.SetProcessing(true);
        }

        try
        {
            await Swap(tile1, tile2);

            if (_matchDetector.CanPop())
            {
                while (_matchDetector.CanPop())
                {
                    await _popHandler.Pop();
                }
            }
            else
            {
                await Swap(tile1, tile2);
            }
        }
        finally
        {
            // Pop 처리 완료
            if (_board != null)
            {
                _board.SetProcessing(false);
            }
        }
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        if (tile1 == null || tile2 == null) return;
        if (!tile1.button.interactable || !tile2.button.interactable) return;
        if (tile1.Item == null || tile2.Item == null) return;

        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var t1 = icon1.transform;
        var t2 = icon2.transform;

        Vector3 p1 = t1.position;
        Vector3 p2 = t2.position;

        Item item1 = tile1.Item;
        Item item2 = tile2.Item;

        if (t1.parent != null)
        {
            t1.SetAsLastSibling();
        }
        if (t2.parent != null)
        {
            t2.SetAsLastSibling();
        }

        var seq = DOTween.Sequence();
        seq.Join(t1.DOMove(p2, TweenDuration));
        seq.Join(t2.DOMove(p1, TweenDuration));

        await seq.Play().AsyncWaitForCompletion();

        t1.position = p1;
        t2.position = p2;

        TileItemSetter.SetTileItem(tile1, item2);
        TileItemSetter.SetTileItem(tile2, item1);
    }
}
