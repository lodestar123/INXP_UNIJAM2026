using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타일 팝 처리 및 애니메이션을 담당하는 클래스
/// </summary>
public class PopHandler
{
    private readonly Tile[,] _tiles;
    private readonly MatchDetector _matchDetector;
    private readonly GravityHandler _gravityHandler;
    private readonly Board _boardOwner;
    private readonly AudioSource _audioSource;
    private readonly AudioClip _collectSound;
    private const float TweenDuration = 1f; // 애니메이션 duration (오른쪽 아래로 이동하는 시간)

    public PopHandler(Tile[,] tiles, MatchDetector matchDetector, GravityHandler gravityHandler, Board boardOwner)
    {
        _tiles = tiles;
        _matchDetector = matchDetector;
        _gravityHandler = gravityHandler;
        _boardOwner = boardOwner;
    }

    private bool OwnerBoardIsActiveSession()
    {
        return _boardOwner != null && _boardOwner.isActiveAndEnabled;
    }

    private bool CanApplyMatchScore(bool allowScore)
    {
        if (!allowScore) return false;
        return OwnerBoardIsActiveSession();
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
        if (GameSceneManager.Instance != null && score > 0 && CanApplyMatchScore(allowScore))
        {
            GameSceneManager.Instance.AddScore(score, forceAddScore: true);
            //Debug.Log($"[PopHandler] {matchedCount}개 타일 매치, 점수: {score}점 (총 점수: {GameSceneManager.Instance.CurrentScore}점)");
        }

        // 1단계: 포장 연출 - 원래 아이템이 작아지며 사라지고 선물상자로 포장
        float packagingDuration = 0.3f; // 포장 애니메이션 시간
        var packagingMaster = DOTween.Sequence();
        packagingMaster.Pause();
        bool hasPackaging = false;

        foreach (var t in matched)
        {
            if (t == null || !t.button.interactable) continue;
            if (t.Item == null || t.icon == null) continue;

            // pop 스프라이트가 있으면 포장 연출
            if (t.Item.spritePop != null)
            {
                var iconTransform = t.icon.transform;

                // 다른 트윈(스왑/중력)과 겹치지 않도록 기존 트윈을 정리하고 상태를 초기화
                iconTransform.DOKill();

                // 페이드아웃을 위한 CanvasGroup
                CanvasGroup canvasGroup = t.icon.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = t.icon.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.DOKill();
                canvasGroup.alpha = 1f;
                iconTransform.localScale = Vector3.one;

                // 포장 시퀀스: 원래 아이템 작아지며 사라짐 → 선물상자 커지며 나타남
                // Pause()로 개별 시퀀스가 만들자마자 실행되지 않게 하고, 마스터 시퀀스에서 한 번에 Play
                Sequence packagingSequence = DOTween.Sequence();

                // 원래 아이템 작아지며 페이드아웃
                packagingSequence.Append(iconTransform.DOScale(Vector3.zero, packagingDuration * 0.6f).SetEase(Ease.InBack));
                packagingSequence.Join(canvasGroup.DOFade(0f, packagingDuration * 0.6f).SetEase(Ease.InQuad));

                // 스프라이트를 선물상자로 교체 (작은 상태로 시작)
                packagingSequence.AppendCallback(() =>
                {
                    if (t.icon != null && t.Item != null && t.Item.spritePop != null)
                    {
                        t.icon.sprite = t.Item.spritePop;
                        iconTransform.localScale = Vector3.zero;
                        canvasGroup.alpha = 0f;
                    }
                });

                // 선물상자가 커지며 나타남 (포장 완료)
                packagingSequence.Append(iconTransform.DOScale(Vector3.one, packagingDuration * 0.4f).SetEase(Ease.OutBack));
                packagingSequence.Join(canvasGroup.DOFade(1f, packagingDuration * 0.4f).SetEase(Ease.OutQuad));

                packagingSequence.Pause();
                packagingMaster.Join(packagingSequence);
                hasPackaging = true;
            }
        }

        // 포장 연출을 실제로 실행하고 완료까지 대기 (AppendInterval만 쓰면 트윈이 안 돌거나 콜백이 누락될 수 있음)
        if (hasPackaging)
        {
            await packagingMaster.Play().AsyncWaitForCompletion();

            // 트윈이 중간에 끊겨도 날아가는 아이콘이 선물상자 스프라이트를 쓰도록 보장
            foreach (var t in matched)
            {
                if (t == null || t.Item == null || t.icon == null) continue;
                if (t.Item.spritePop != null)
                {
                    t.icon.sprite = t.Item.spritePop;
                    t.icon.transform.localScale = Vector3.one;
                    CanvasGroup cg = t.icon.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;
                }
            }
        }

        // 사운드 재생 
        if (GameManager.Instance != null && OwnerBoardIsActiveSession())
        {
            GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ThreeMatch);
        }

        await Task.Delay(120); // 포장 후 잠시 대기

        // 날아가는 선물상자를 타일 아이콘과 분리하기 위해 매치된 아이콘을 복제 생성함
        // 타일의 원래 아이콘은 즉시 비워져, 선물상자가 날아가기 시작하자마자 중력이 곧바로 동시에 진행된다
        var deflate = DOTween.Sequence();
        var flyers = new List<GameObject>();

        foreach (var t in matched)
        {
            if (t == null || !t.button.interactable) continue;
            if (t.Item == null) continue;
            if (t.icon == null) continue;

            RectTransform sourceRect = t.icon.rectTransform;
            if (sourceRect == null) continue;

            Vector2 startPos = sourceRect.anchoredPosition;

            // 목적지까지의 "이동량"을 루트 캔버스/렌더 카메라로 일관되게 계산해 시작 위치에 더한다
            Vector2 endPos = startPos + CalculatePopFlightDelta(sourceRect);

            // 매치된 아이콘을 복제해 플라이어 생성
            Image flyer = Object.Instantiate(t.icon, sourceRect.parent);
            RectTransform flyerRect = flyer.rectTransform;
            flyerRect.anchoredPosition = startPos;
            flyerRect.localScale = sourceRect.localScale;
            flyer.sprite = t.icon.sprite;
            flyer.transform.SetAsLastSibling();

            CanvasGroup flyerGroup = flyer.GetComponent<CanvasGroup>();
            if (flyerGroup == null)
            {
                flyerGroup = flyer.gameObject.AddComponent<CanvasGroup>();
            }
            flyerGroup.alpha = 1f;

            flyers.Add(flyer.gameObject);

            // 타일의 원래 아이콘/데이터를 즉시 비우고 중력이 이 자리를 곧바로 재사용 가능
            TileItemSetter.SetTileItem(t, null);
            t.icon.transform.localScale = Vector3.one;
            CanvasGroup tileGroup = t.icon.GetComponent<CanvasGroup>();
            if (tileGroup != null)
            {
                tileGroup.alpha = 1f;
            }

            // 포물선 경로: 아래로 포물선을 그리며 떨어지는 효과
            Vector2 midPoint = (startPos + endPos) * 0.5f;
            float arcDepth = 100f; // 포물선의 깊이 (아래로)
            Vector2 lowestPos = new Vector2(midPoint.x, Mathf.Min(startPos.y, endPos.y) - arcDepth);

            // 애니메이션 duration 설정
            float moveDuration = duration;
            float scaleDuration = duration * 0.8f;
            float fadeDuration = duration * 0.85f;

            // 포물선 애니메이션: X는 부드럽게 이동, Y는 아래로 포물선을 그리며 떨어짐
            deflate.Join(flyerRect.DOAnchorPosX(endPos.x, moveDuration).SetEase(Ease.OutQuad));
            Sequence ySequence = DOTween.Sequence();
            ySequence.Append(flyerRect.DOAnchorPosY(lowestPos.y, moveDuration * 0.5f).SetEase(Ease.OutQuad));
            ySequence.Append(flyerRect.DOAnchorPosY(endPos.y, moveDuration * 0.5f).SetEase(Ease.InQuad));
            deflate.Join(ySequence);

            // 페이드아웃 + 스케일 축소
            deflate.Join(flyerGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
            deflate.Join(flyer.transform.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.InBack));
        }

        // 선물상자가 날아가는 연출은 백그라운드(fire-and-forget)로 실행하고, 끝나면 스스로 플라이어를 정리한다.
        // Pop()은 타일이 내려오는 중력만 기다리므로, 다음 pop(연쇄/두 번째 스왑)이
        // 이전 pop의 비행 연출이 끝나길 기다리지 않고 곧바로 시작될 수 있다.
        if (flyers.Count > 0)
        {
            PlayFlyAnimationAndCleanup(deflate, flyers);
        }

        // 타일이 내려오는 연출(중력)만 기다린다.
        await _gravityHandler.ApplyGravityOnly();

        return true;
    }

    /// <summary>
    /// 날아가는 선물상자(플라이어) 연출을 백그라운드로 재생하고, 완료되면 플라이어를 정리한다.
    /// Pop() 흐름과 분리되어 있어 다음 pop이 이 연출을 기다리지 않는다.
    /// </summary>
    private async void PlayFlyAnimationAndCleanup(Sequence deflate, List<GameObject> flyers)
    {
        try
        {
            await deflate.Play().AsyncWaitForCompletion();
        }
        finally
        {
            foreach (var flyer in flyers)
            {
                if (flyer != null)
                {
                    Object.Destroy(flyer);
                }
            }
        }
    }

    /// <summary>
    /// 팝된 선물상자가 시작 위치에서 목적지까지 날아갈 "이동량"(부모 로컬 기준)을 계산한다
    /// </summary>
    private Vector2 CalculatePopFlightDelta(RectTransform sourceRect)
    {
        // 좌표 변환 기준을 루트 Canvas로 고정
        Canvas rootCanvas = sourceRect.GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            rootCanvas = rootCanvas.rootCanvas;
        }
        RectTransform rootRect = rootCanvas != null ? rootCanvas.transform as RectTransform : null;
        // 루트 Canvas의 렌더 카메라(Screen Space - Camera). WorldToScreen / ScreenToLocal 양쪽에 동일하게 사용
        Camera cam = rootCanvas != null ? rootCanvas.worldCamera : null;

        // 시작점(아이콘)의 루트 캔버스 로컬 좌표
        Vector2 sourceLocal = WorldToCanvasLocal(sourceRect.position, rootRect, cam);

        // Board에 지정된 목적지 오브젝트가 있으면 그 위치로
        if (Board.Instance != null && Board.Instance.PopDestinationTarget != null && rootRect != null)
        {
            Vector2 destLocal = WorldToCanvasLocal(Board.Instance.PopDestinationTarget.position, rootRect, cam);
            return destLocal - sourceLocal;
        }

        // 목적지 오브젝트가 없으면 Canvas 오른쪽 아래 모서리로 향함
        if (rootRect != null)
        {
            Vector2 corner = new Vector2(rootRect.rect.width * 0.5f - 50f, -rootRect.rect.height * 0.5f + 50f);
            return corner - sourceLocal;
        }

        // 기본값
        return new Vector2(750f, -300f);
    }

    /// <summary>
    /// 월드 좌표를 루트 Canvas 로컬 좌표(anchoredPosition과 같은 단위/축)로 변환한다
    /// </summary>
    private static Vector2 WorldToCanvasLocal(Vector3 worldPosition, RectTransform rootRect, Camera cam)
    {
        if (rootRect == null)
        {
            return Vector2.zero;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, screenPoint, cam, out Vector2 local);
        return local;
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
                    return matchedCount * 300;
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
