using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 보드 채우기 시스템 - cursor 기반으로 아이템을 순서대로 배치
/// 아이템 리스트의 순서를 절대 변경하지 않고, cursor 위치부터 채웁니다.
/// </summary>
public class BoardFillSystem
{
    private readonly Row[] _rows;
    private readonly Tile[,] _tiles;
    private readonly int _boardWidth;
    private readonly int _boardHeight;
    private readonly BoardFillCursor _fillCursor;
    private readonly ItemQueueManager _itemQueueManager;

    public BoardFillSystem(
        Row[] rows, 
        Tile[,] tiles, 
        BoardFillCursor fillCursor,
        ItemQueueManager itemQueueManager)
    {
        _rows = rows;
        _tiles = tiles;
        _boardWidth = tiles.GetLength(0);
        _boardHeight = tiles.GetLength(1);
        _fillCursor = fillCursor;
        _itemQueueManager = itemQueueManager;
    }

    /// <summary>
    /// 보드를 채웁니다 (cursor 위치부터 이어서 채움)
    /// 아이템이 부족하면 빈칸으로 남깁니다.
    /// </summary>
    public void FillBoard()
    {
        // 타일 좌표 초기화
        InitializeTilePositions();

        // cursor 위치부터 채우기 시작
        int startIndex = _fillCursor.CurrentIndex;
        int totalSlots = _boardWidth * _boardHeight;
        int remainingSlots = totalSlots - startIndex;

        if (remainingSlots <= 0)
        {
            Debug.Log("[BoardFillSystem] 보드가 이미 모두 채워져 있습니다.");
            return;
        }

        // ItemQueue에서 필요한 만큼 아이템 가져오기 (제거하지 않음)
        List<Item> availableItems = _itemQueueManager.PeekItems(remainingSlots);
        int itemCount = availableItems.Count;

        Debug.Log($"[BoardFillSystem] 채우기 시작 - 시작 인덱스: {startIndex}, 남은 칸: {remainingSlots}, 사용 가능한 아이템: {itemCount}");

        // cursor 위치부터 순서대로 채우기
        // 기획: 왼쪽 아래(0,0) → 오른쪽 → 위 순서
        // Unity UI에서 Row[0]이 위쪽이므로 y를 역순으로 매핑
        int itemIndex = 0;
        
        for (int index = startIndex; index < totalSlots; index++)
        {
            int x = index % _boardWidth;
            int rowIndex = index / _boardWidth; // 0부터 시작 (위에서 아래)
            int y = _boardHeight - 1 - rowIndex; // 역순으로 매핑 (아래에서 위)

            var tile = _rows[y].tiles[x];

            // 아이템이 있으면 배치, 없으면 빈칸
            if (itemIndex < itemCount)
            {
                tile.Item = availableItems[itemIndex];
                tile.button.interactable = true; // 상호작용 가능
                tile.icon.gameObject.SetActive(true);
                tile.icon.sprite = availableItems[itemIndex].sprite;
                tile.icon.transform.localScale = Vector3.one;
                itemIndex++;
            }
            else
            {
                // 아이템이 부족하면 빈 칸으로 유지
                tile.Item = null;
                tile.button.interactable = false; // 상호작용 불가능
                tile.icon.gameObject.SetActive(false); // 아이콘 숨김
            }

            _tiles[x, y] = tile;
        }

        // 실제로 사용한 아이템 개수만큼 큐에서 제거
        if (itemCount > 0)
        {
            _itemQueueManager.RemoveItems(itemCount);
            _fillCursor.MoveNext(itemCount);
        }

        Debug.Log($"[BoardFillSystem] 채우기 완료 - {itemCount}개 아이템 배치, {remainingSlots - itemCount}개 빈칸, 커서 위치: {_fillCursor.CurrentIndex}");
    }

    /// <summary>
    /// 보드를 처음부터 다시 채웁니다 (cursor 리셋)
    /// </summary>
    public void FillBoardFromStart()
    {
        _fillCursor.Reset();
        FillBoard();
    }

    /// <summary>
    /// 보드를 완전히 비웁니다 (모든 타일을 빈칸으로)
    /// </summary>
    public void ClearBoard()
    {
        for (int y = 0; y < _boardHeight; y++)
        {
            for (int x = 0; x < _boardWidth; x++)
            {
                var tile = _rows[y].tiles[x];
                tile.Item = null;
                tile.button.interactable = false;
                tile.icon.gameObject.SetActive(false);
                _tiles[x, y] = tile;
            }
        }

        _fillCursor.Reset();
        Debug.Log("[BoardFillSystem] 보드 초기화 완료");
    }

    /// <summary>
    /// 타일들의 좌표를 초기화합니다
    /// </summary>
    private void InitializeTilePositions()
    {
        for (int y = 0; y < _boardHeight; y++)
        {
            for (int x = 0; x < _boardWidth; x++)
            {
                var tile = _rows[y].tiles[x];
                tile.x = x;
                tile.y = y;
            }
        }
    }

    /// <summary>
    /// 현재 보드 상태를 확인합니다
    /// </summary>
    public void DebugPrintBoardState()
    {
        int filledCount = 0;
        int emptyCount = 0;

        for (int y = 0; y < _boardHeight; y++)
        {
            for (int x = 0; x < _boardWidth; x++)
            {
                if (_tiles[x, y] != null && _tiles[x, y].Item != null)
                    filledCount++;
                else
                    emptyCount++;
            }
        }

        Debug.Log($"[BoardFillSystem] 보드 상태 - 채워진 칸: {filledCount}, 빈 칸: {emptyCount}, 커서 위치: {_fillCursor.CurrentIndex}");
    }
}
