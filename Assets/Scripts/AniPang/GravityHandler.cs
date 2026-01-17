using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 보드에 중력을 적용하는 클래스
/// </summary>
public class GravityHandler
{
    private readonly Tile[,] _tiles;
    private readonly int _width;
    private readonly int _height;

    public GravityHandler(Tile[,] tiles)
    {
        _tiles = tiles;
        _width = tiles.GetLength(0);
        _height = tiles.GetLength(1);
    }

    /// <summary>
    /// 중력만 적용 (리필 없음)
    /// </summary>
    public async Task ApplyGravityOnly()
    {
        for (int x = 0; x < _width; x++)
        {
            // 1) 위 -> 아래로 남아있는 아이템 수집
            var remain = new List<Item>();
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null || !t.button.interactable) continue;

                if (t.Item != null) remain.Add(t.Item);
            }

            // 2) 위 -> 아래로 채우기 (button.interactable이 true인 타일만 채움)
            int idx = 0;
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null) continue;
                
                // button.interactable이 false인 타일은 그대로 유지 (빈 영역)
                if (!t.button.interactable) continue;

                if (idx < remain.Count)
                {
                    TileItemSetter.SetTileItem(t, remain[idx]);
                    idx++;
                }
                else
                {
                    // 남는 아래쪽은 빈칸
                    TileItemSetter.SetTileItem(t, null);
                }
            }
        }

        // 중력 적용 후 정리: 상태 일관성 보장
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var t = _tiles[x, y];
                if (t == null) continue;

                // button.interactable이 false인 타일은 항상 Item이 null이어야 함 (빈 칸)
                if (!t.button.interactable)
                {
                    if (t.Item != null)
                    {
                        t.Item = null;
                        t.icon.gameObject.SetActive(false);
                    }
                }
                // Item이 null이고 button.interactable이 true인 경우 (Pop된 빈칸)
                // button.interactable을 false로 설정
                else if (t.Item == null)
                {
                    t.button.interactable = false;
                    t.icon.gameObject.SetActive(false);
                }
            }
        }

        await Task.CompletedTask;
    }
}
