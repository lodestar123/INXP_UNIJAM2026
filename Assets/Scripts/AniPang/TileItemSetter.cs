using UnityEngine;

/// <summary>
/// 타일의 아이템을 설정하는 유틸리티 클래스
/// </summary>
public static class TileItemSetter
{
    /// <summary>
    /// 타일의 아이템을 설정하고 시각적 표현을 업데이트합니다
    /// </summary>
    public static void SetTileItem(Tile tile, Item item)
    {
        tile.Item = item;

        // 빈 타일(버튼 비활성) 영역은 건드리지 않음
        if (!tile.button.interactable)
        {
            tile.Item = null;
            tile.icon.gameObject.SetActive(false);
            return;
        }

        if (item == null)
        {
            tile.icon.gameObject.SetActive(false);
            return;
        }

        tile.icon.gameObject.SetActive(true);
        tile.icon.sprite = item.sprite; // Item에 sprite가 있다고 가정 (없으면 네 구조에 맞게 수정)
        tile.icon.transform.localScale = Vector3.one;
    }
}
