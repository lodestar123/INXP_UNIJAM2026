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
    private const float FallDuration = 0.18f; // 아이템이 아래로 미끄러져 떨어지는 시간
    private const float BounceOvershoot = 1.1f; // 착지 시 바운스 강도

    public GravityHandler(Tile[,] tiles)
    {
        _tiles = tiles;
        _width = tiles.GetLength(0);
        _height = tiles.GetLength(1);
    }

    /// <summary>
    /// 중력만 적용 - 아이템이 아래로 미끄러져 떨어지는 연출
    /// </summary>
    public async Task ApplyGravityOnly()
    {
        // 이동할 아이콘과 도착 위치를 모아 한 번에 떨어뜨린다
        var moves = new List<(Transform iconTransform, Vector3 homePos)>();

        for (int x = 0; x < _width; x++)
        {
            // 1) 남아있는 아이템을 아래->위 순서로 수집 (원래 y 기억)
            var falling = new List<(Item item, int originalY)>();
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null || !t.button.interactable) continue;

                if (t.Item != null)
                {
                    falling.Add((t.Item, y));
                }
            }

            // 2) 아래->위 순서로 타일에 다시 채우면서, 이동한 아이템은 떨어지는 연출 준비
            int idx = 0;
            for (int y = _height - 1; y >= 0; y--)
            {
                var t = _tiles[x, y];
                if (t == null) continue;
                if (!t.button.interactable) continue;

                if (idx < falling.Count)
                {
                    var (item, originalY) = falling[idx];
                    TileItemSetter.SetTileItem(t, item);

                    // 위치가 바뀐 아이템만: 아이콘을 원래(위쪽) 위치에서 시작시켜 제자리로 떨어뜨림
                    if (originalY != y && t.icon != null)
                    {
                        Vector3 startWorldPos = _tiles[x, originalY].transform.position;
                        Vector3 homePos = t.transform.position;

                        t.icon.transform.localScale = Vector3.one;
                        t.icon.transform.position = startWorldPos;
                        moves.Add((t.icon.transform, homePos));
                    }
                    idx++;
                }
                else
                {
                    TileItemSetter.SetTileItem(t, null);
                }
            }
        }

        // 3) 모든 이동 아이템을 동시에 아래로 미끄러뜨림 (떨어지는 느낌으로 가속)
        if (moves.Count > 0)
        {
            var fallSequence = DOTween.Sequence();
            foreach (var (iconTransform, homePos) in moves)
            {
                // OutBack + 작은 overshoot: 착지 시 미세하게 튀게
                fallSequence.Join(iconTransform.DOMove(homePos, FallDuration).SetEase(Ease.OutBack, BounceOvershoot));
            }
            await fallSequence.Play().AsyncWaitForCompletion();

            // 정위치 보정: 애니메이션 오차 없이 셀 중앙으로
            foreach (var (iconTransform, homePos) in moves)
            {
                iconTransform.position = homePos;
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
