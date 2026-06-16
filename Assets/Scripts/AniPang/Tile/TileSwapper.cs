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
    private const float TweenDuration = 0.25f;

    // 팝+중력 루프가 이미 돌고 있는지 여부
    // 입력은 막지 않고, 새로 커밋된 스왑의 매치는 진행 중인 루프가 이어서 처리함
    private bool _resolving = false;

    // 현재 스왑(교환+되돌리기) 처리 중인 타일 집합
    private readonly HashSet<Tile> _busyTiles = new HashSet<Tile>();

    public TileSwapper(Tile[,] tiles, MatchDetector matchDetector, PopHandler popHandler)
    {
        _tiles = tiles;
        _matchDetector = matchDetector;
        _popHandler = popHandler;
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

        var first = _selection[0];
        var second = _selection[1];
        _selection.Clear();

        await TrySwapAndResolve(first, second);
    }

    public async void SwapTiles(Tile tile1, Tile tile2)
    {
        if (tile1 == null || tile2 == null) return;
        if (tile1.Item == null || tile2.Item == null) return;
        if (!tile1.button.interactable || !tile2.button.interactable) return;
        
        _selection.Clear();

        await TrySwapAndResolve(tile1, tile2);
    }

    /// <summary>
    /// 두 타일을 교환하고, 교환으로 매치가 생기면 백그라운드 매치 해소를 시작한다
    /// </summary>
    private async Task TrySwapAndResolve(Tile a, Tile b)
    {
        if (a == null || b == null) return;

        // 두 타일 중 하나라도 이미 다른 스왑이 처리 중이면 무시한다(겹침 방지).
        if (_busyTiles.Contains(a) || _busyTiles.Contains(b)) return;

        _busyTiles.Add(a);
        _busyTiles.Add(b);
        try
        {
            await Swap(a, b);

            // 교환한 두 타일 중 하나라도 매치에 포함되면 유효한 수로 보고 해소를 시작한다
            var matched = _matchDetector.GetAllMatchedTiles();
            if (matched.Contains(a) || matched.Contains(b))
            {
                ResolveMatches();
            }
            else
            {
                // 매치가 없으면 원위치로 되돌린다
                await Swap(a, b);
            }
        }
        finally
        {
            _busyTiles.Remove(a);
            _busyTiles.Remove(b);
        }
    }

    /// <summary>
    /// 보드에 남아있는 모든 매치를 연쇄적으로 해소한다
    /// 이미 해소 루프가 돌고 있으면 중복 실행하지 않으며,
    /// 진행 중인 루프가 그 사이 새로 만들어진 매치까지 이어서 처리한다.
    /// </summary>
    private async void ResolveMatches()
    {
        if (_resolving) return;

        _resolving = true;
        try
        {
            while (_matchDetector.CanPop())
            {
                await _popHandler.Pop();
            }
        }
        finally
        {
            _resolving = false;
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

        // 절대 움직이지 않는 타일(셀)의 위치를 홈 좌표로 사용
        // 아이콘은 타일 중앙에 정렬된 자식이므로 타일의 월드 위치 = 아이콘의 정위치
        Vector3 home1 = tile1.transform.position;
        Vector3 home2 = tile2.transform.position;

        Item item1 = tile1.Item;
        Item item2 = tile2.Item;

        // 데이터 교환을 await(애니메이션) 이전에 동기적으로 커밋한다
        TileItemSetter.SetTileItem(tile1, item2);
        TileItemSetter.SetTileItem(tile2, item1);

        if (t1.parent != null)
        {
            t1.SetAsLastSibling();
        }
        if (t2.parent != null)
        {
            t2.SetAsLastSibling();
        }

        // 연출: 교환된 두 아이콘이 서로의 자리에서 출발해 자기 셀로 미끄러져 들어옴
        t1.position = home2;
        t2.position = home1;

        var seq = DOTween.Sequence();
        seq.Join(t1.DOMove(home1, TweenDuration));
        seq.Join(t2.DOMove(home2, TweenDuration));

        await seq.Play().AsyncWaitForCompletion();

        // 애니메이션이 끝나면 각 아이콘을 자기 셀의 정위치로 되돌림
        t1.position = home1;
        t2.position = home2;
    }
}
