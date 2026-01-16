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
            // 1) 위 -> 아래로 남아있는 아이템 수집 (순서 유지 핵심)
            var remain = new List<Item>();
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null || !t.button.interactable) continue;

                if (t.Item != null) remain.Add(t.Item);
            }

            // 2) 위 -> 아래로 채우기 (중력 반대)
            int idx = 0;
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null || !t.button.interactable) continue;

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

        // 중력 적용 후, Item이 null인 타일들의 button.interactable을 false로 설정 (Pop된 빈칸 정리)
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var t = _tiles[x, y];
                if (t == null) continue;

                // Item이 null이고 button.interactable이 true인 경우 (Pop된 빈칸)
                // button.interactable을 false로 설정
                if (t.Item == null && t.button.interactable)
                {
                    t.button.interactable = false;
                    t.icon.gameObject.SetActive(false);
                }
            }
        }

        await Task.CompletedTask;
    }
}
