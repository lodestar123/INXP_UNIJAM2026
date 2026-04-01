using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 보드 초기화를 담당하는 클래스
/// </summary>
public class BoardInitializer
{
    private readonly Row[] _rows;
    private readonly Tile[,] _tiles;
    private readonly MatchDetector _matchDetector;
    private readonly bool _useTestItems;
    private readonly List<Item> _testItems;

    public BoardInitializer(Row[] rows, Tile[,] tiles, MatchDetector matchDetector, bool useTestItems, List<Item> testItems)
    {
        _rows = rows;
        _tiles = tiles;
        _matchDetector = matchDetector;
        _useTestItems = useTestItems;
        _testItems = testItems;
    }

    /// <summary>
    /// 초기 보드를 생성합니다
    /// </summary>
    public void CreateInitialBoard()
    {
        int boardWidth = _rows.Max(row => row.tiles.Length);
        int boardHeight = _rows.Length;

        // 테스트 모드인 경우 테스트 아이템 사용
        List<Item> collectedItems;
        if (_useTestItems && _testItems.Count > 0)
        {
            collectedItems = new List<Item>(_testItems);
            Debug.Log($"[테스트 모드] 테스트 아이템 {collectedItems.Count}개 사용");
        }
        else
        {
            // 플래피버드에서 수집한 아이템 리스트 가져오기
            collectedItems = FlappyItemCollector.GetCollectedItems();
        }
        
        // 수집한 아이템이 없으면 기존 랜덤 방식 사용
        if (collectedItems.Count == 0)
        {
            CreateRandomBoard();
            return;
        }

        // 수집 아이템을 선형 인덱스로 배치합니다.
        // 주의: 여기서는 y = index / width(위→아래 Row 순)입니다. 큐 기준 채우기(BoardFillSystem)의
        // FillOrderIndexToCell(아래→위)와 규칙이 다릅니다. 동작을 바꾸지 않기 위해 기존 매핑을 유지합니다.
        int totalTiles = boardWidth * boardHeight;
        int itemIndex = 0;

        for (int index = 0; index < totalTiles; index++)
        {
            int x = index % boardWidth;
            int y = index / boardWidth;

            var tile = _rows[y].tiles[x];
            SetTileGridCell(tile, x, y);
            
            // 수집한 아이템이 있으면 배치, 없으면 빈 칸
            if (itemIndex < collectedItems.Count)
            {
                tile.Item = collectedItems[itemIndex];
                tile.button.interactable = true; // 상호작용 가능
                itemIndex++;
            }
            else
            {
                tile.Item = null; // 빈 칸
                tile.button.interactable = false; // 상호작용 불가능
                tile.icon.gameObject.SetActive(false); // 아이콘 숨김
            }
            
            _tiles[x, y] = tile;
        }
        
        Debug.Log($"[보드 생성] {collectedItems.Count}개 아이템 배치 완료, 빈 칸 {totalTiles - collectedItems.Count}개");
    }

    /// <summary>
    /// 랜덤 보드를 생성합니다
    /// </summary>
    private void CreateRandomBoard()
    {
        int width = _tiles.GetLength(0);
        int height = _tiles.GetLength(1);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = _rows[y].tiles[x];
                SetTileGridCell(tile, x, y);
                tile.button.interactable = true; // 상호작용 가능

                do
                {
                    tile.Item = ItemDataBase.Items[Random.Range(0, ItemDataBase.Items.Length)];
                } while (_matchDetector.IsConsecutiveTile(x, y, tile.Item));

                _tiles[x, y] = tile;
            }
        }
    }

    static void SetTileGridCell(Tile tile, int x, int y)
    {
        tile.x = x;
        tile.y = y;
    }
}
