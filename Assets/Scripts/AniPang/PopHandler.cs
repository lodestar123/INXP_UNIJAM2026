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
    private const float TweenDuration = 1f; // 애니메이션 duration (오른쪽 아래로 이동하는 시간)

    public PopHandler(Tile[,] tiles, MatchDetector matchDetector, GravityHandler gravityHandler)
    {
        _tiles = tiles;
        _matchDetector = matchDetector;
        _gravityHandler = gravityHandler;
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

        // 1단계: 스프라이트를 pop 스프라이트로 교체
        foreach (var t in matched)
        {
            if (t == null || !t.button.interactable) continue;
            if (t.Item == null) continue;
            
            // pop 스프라이트가 있으면 교체
            if (t.Item.sprite_Pop != null)
            {
                t.icon.sprite = t.Item.sprite_Pop;
            }
        }
        
        await Task.Delay(100);
        
        // 사운드 재생 (애니메이션과 병렬)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ThreeMatch);
            //1초 기다림
            await Task.Delay(1000);
            GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.AddScore);
        }
        
        var deflate = DOTween.Sequence();
        
        // 각 타일의 원래 위치를 저장 (복원용)
        var tilePositions = new System.Collections.Generic.Dictionary<Tile, Vector2>();

        foreach (var t in matched)
        {
            if (t == null || !t.button.interactable) continue;
            if (t.Item == null) continue;
            if (t.icon == null) continue;

            RectTransform rectTransform = t.icon.rectTransform;
            if (rectTransform == null) continue;
         
            Vector2 startPos = rectTransform.anchoredPosition;
            tilePositions[t] = startPos;

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            RectTransform canvasRect = canvas != null ? canvas.transform as RectTransform : null;
            
            Vector2 endPos = new Vector2(700f, -300f); 
            
            // 포물선 경로: 아래로 포물선을 그리며 떨어지는 효과
            Vector2 midPoint = (startPos + endPos) * 0.5f;
            float arcDepth = 100f; // 포물선의 깊이 (아래로)
            Vector2 lowestPos = new Vector2(midPoint.x, Mathf.Min(startPos.y, endPos.y) - arcDepth);
            
            // 애니메이션 duration 설정
            float moveDuration = duration;
            float scaleDuration = duration * 0.8f;
            float fadeDuration = duration * 0.85f; 
            
            // 포물선 애니메이션: X는 부드럽게 이동, Y는 아래로 포물선을 그리며 떨어짐
            // X축: 시작 -> 끝 (부드럽게)
            deflate.Join(rectTransform.DOAnchorPosX(endPos.x, moveDuration).SetEase(Ease.OutQuad));
            // Y축: 시작 -> 최저점 -> 끝 (아래로 포물선)
            Sequence ySequence = DOTween.Sequence();
            ySequence.Append(rectTransform.DOAnchorPosY(lowestPos.y, moveDuration * 0.5f).SetEase(Ease.OutQuad));
            ySequence.Append(rectTransform.DOAnchorPosY(endPos.y, moveDuration * 0.5f).SetEase(Ease.InQuad));
            deflate.Join(ySequence);
            
            // 페이드아웃: 투명하게 사라짐
            CanvasGroup canvasGroup = t.icon.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = t.icon.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 1f;
            deflate.Join(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
            
            // 스케일 애니메이션: 이동하면서 서서히 작아짐
            deflate.Join(t.icon.transform.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.InBack));
        }
        
        // 애니메이션 완료 대기
        await deflate.Play().AsyncWaitForCompletion();
        
        // 점수 사운드는 애니메이션 후에 재생
        if (GameManager.Instance != null)
        {
            //GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.AddScore);
        }

        foreach (var t in matched)
        {
            if (t == null || !t.button.interactable) continue;
            
            if (t.icon != null && tilePositions.TryGetValue(t, out Vector2 originalPos))
            {
                RectTransform rectTransform = t.icon.rectTransform;
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = originalPos;
                }
                
                // 페이드아웃 복원
                CanvasGroup canvasGroup = t.icon.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
                else
                {
                    Color color = t.icon.color;
                    color.a = 1f;
                    t.icon.color = color;
                }
                
                // 스케일 복원
                t.icon.transform.localScale = Vector3.one;
            }
            
            TileItemSetter.SetTileItem(t, null);
        }

        // 중력 적용 (리필 없음 - 기획에 따라)
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
