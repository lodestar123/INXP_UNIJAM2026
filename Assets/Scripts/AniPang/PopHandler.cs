using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 타일 팝 처리 및 애니메이션을 담당하는 클래스
/// </summary>
public class PopHandler
{
    private readonly Tile[,] _tiles;
    private readonly MatchDetector _matchDetector;
    private readonly GravityHandler _gravityHandler;
    private readonly AudioSource _audioSource;
    private readonly AudioClip _collectSound;
    private const float TweenDuration = 0.25f;

    public PopHandler(Tile[,] tiles, MatchDetector matchDetector, GravityHandler gravityHandler, AudioSource audioSource, AudioClip collectSound)
    {
        _tiles = tiles;
        _matchDetector = matchDetector;
        _gravityHandler = gravityHandler;
        _audioSource = audioSource;
        _collectSound = collectSound;
    }

    /// <summary>
    /// 매칭된 타일들을 팝 처리
    /// </summary>
    /// <param name="allowScore">점수 계산 허용 여부</param>
    /// <param name="animationDuration">애니메이션 지속 시간 (기본값: 0.25초)</param>
    public async Task<bool> Pop(bool allowScore = true, float animationDuration = -1f)
    {
        var matched = _matchDetector.GetAllMatchedTiles();
        if (matched.Count == 0) return false;

        // animationDuration이 -1이면 기본값 사용
        float duration = animationDuration < 0 ? TweenDuration : animationDuration;

        int matchedCount = matched.Count;
        int score = CalculateScore(matchedCount);
        
        // GameSceneManager에 점수 추가
        if (GameSceneManager.Instance != null && score > 0 && allowScore)
        {
            GameSceneManager.Instance.AddScore(score, forceAddScore: true);
            //Debug.Log($"[PopHandler] {matchedCount}개 타일 매치, 점수: {score}점 (총 점수: {GameSceneManager.Instance.CurrentScore}점)");
        }

        // 터질 타일들 Deflate
        var deflate = DOTween.Sequence();

        foreach (var t in matched)
        {
            if (t == null || !t.button.interactable) continue;
            if (t.Item == null) continue;

            deflate.Join(t.icon.transform.DOScale(Vector3.zero, duration));
        }

        _audioSource.PlayOneShot(_collectSound);
        await deflate.Play().AsyncWaitForCompletion();

        // 실제로 비우기 (Item null)
        foreach (var t in matched)
        {
            if (t == null || !t.button.interactable) continue;
            TileItemSetter.SetTileItem(t, null);
        }

        // 중력 + 리필
        await _gravityHandler.ApplyGravityOnly();

        return true;
    }

    private int CalculateScore(int matchedCount)
    {
        switch (matchedCount)
        {
            case 3:
                return 250;
            case 4:
                return 500;
            case 5:
                return 1000;
            case 6:
                return 2000;
            case 7:
            default:
                if (matchedCount >= 7)
                {
                    return 5000;
                }
                return 0;
        }
    }

    /// <summary>
    /// 연결된 타일들을 팝 처리(현재 미사용)
    /// </summary>
    private async Task PopConnectedTiles(List<Tile> connectedTiles)
    {
        if (connectedTiles.Count >= 3)
        {
            var deflateSequence = DOTween.Sequence();
            var colors = new Dictionary<Item, int>(); // 각 색깔의 타일 개수를 저장할 딕셔너리

            foreach (var connectedTile in connectedTiles)
            {
                // 빈 타일은 건너뛰기
                if (connectedTile == null || connectedTile.Item == null || !connectedTile.button.interactable)
                    continue;
                
                deflateSequence.Join(connectedTile.icon.transform.
                    DOScale(Vector3.zero, TweenDuration));

                // 색깔 별 타일 개수 세기
                if (!colors.ContainsKey(connectedTile.Item))
                {
                    colors[connectedTile.Item] = 1;
                }
                else
                {
                    colors[connectedTile.Item]++;
                }
            }

            _audioSource.PlayOneShot(_collectSound);

            // 각 색깔 별로 개별적으로 점수 계산
            foreach (var colorCount in colors)
            {
                //Score.Instance.AddScore(colorCount.Key, colorCount.Key.value * colorCount.Value);
            }

            await deflateSequence.Play().AsyncWaitForCompletion();

            var inflateSequence = DOTween.Sequence();

            foreach (var connectedTile in connectedTiles)
            {
                // 빈 타일이면 새 아이템 생성하지 않음
                if (connectedTile == null || connectedTile.button.interactable == false)
                {
                    continue;
                }
                
                connectedTile.Item = ItemDataBase.
                    Items[Random.Range(0, ItemDataBase.Items.Length)];

                inflateSequence.Join(connectedTile.icon.transform.
                    DOScale(Vector3.one, TweenDuration));
            }

            await inflateSequence.Play().AsyncWaitForCompletion();
        }
    }
}
