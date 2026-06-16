using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class TapeInsertAnimUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform tape;          // 움직일 UI
    [SerializeField] private Button clickButton;          // 버튼(선택)
    [SerializeField] private GameObject hoverEffectObject; // 마우스 효과 오브젝트

    [Header("Positions (Anchored)")]
    [SerializeField] private Vector2 startPos;     // 시작 위치
    [SerializeField] private Vector2 insertedPos;  // 최종 체결 위치

    [Header("Direction")]
    [SerializeField] private Vector2 forwardDir = Vector2.up; // "앞으로" 방향

    [Header("Timing")]
    [SerializeField] private float backDuration = 0.08f;
    [SerializeField] private float thrustDuration = 0.18f;
    [SerializeField] private float lockDuration = 0.10f;
    [SerializeField] private float snapBackDuration = 0.15f; // 미달 시 복귀 시간

    [Header("Tuning")]
    [SerializeField] private float backDistance = 30f;
    [SerializeField] private float overshoot = 8f;
    [SerializeField] private float punch = 6f;
    [SerializeField] private float punchDuration = 0.08f;

    [Header("Slide")]
    [Range(0.1f, 1f)]
    [SerializeField] private float slideThreshold = 0.5f; // 전체 거리의 몇 %를 넘기면 삽입 발동

    private Sequence seq;
    private bool isPlaying;
    private RectTransform rect;

    // 드래그 상태
    private RectTransform parentRect;
    private bool _dragging;
    private Vector2 _dragStartLocal;
    private float _currentForward;
    private float _maxForward;

    private void Awake()
    {
        if (tape == null) tape = GetComponent<RectTransform>();
        rect = GetComponent<RectTransform>();
        parentRect = tape.parent as RectTransform;

        startPos = tape.anchoredPosition; // 현재를 시작으로
        forwardDir = forwardDir.sqrMagnitude < 0.0001f ? Vector2.up : forwardDir.normalized;

        // 시작→체결 사이의 forwardDir 방향 총 거리(드래그 클램프/임계값 기준)
        _maxForward = Mathf.Max(0.0001f, Vector2.Dot(insertedPos - startPos, forwardDir));

        if (clickButton != null)
        {
            clickButton.onClick.AddListener(PlayAndChangeGame);

            if (hoverEffectObject != null) hoverEffectObject.SetActive(false);

            EventTrigger trigger = clickButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = clickButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entryEnter.callback.AddListener((data) => { if (hoverEffectObject != null) hoverEffectObject.SetActive(true); });
            trigger.triggers.Add(entryEnter);

            EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener((data) => { if (hoverEffectObject != null && !isPlaying && !_dragging) hoverEffectObject.SetActive(false); });
            trigger.triggers.Add(entryExit);
        }

        if (hoverEffectObject != null) hoverEffectObject.SetActive(false);
    }

    void OnEnable()
    {
        seq?.Kill();
        tape.DOKill();

        tape.anchoredPosition = startPos;
        isPlaying = false;
        _dragging = false;
        _currentForward = 0f;
        if (hoverEffectObject != null) hoverEffectObject.SetActive(false);
    }

    // ───────── 클릭 경로 (기존 풀 시퀀스) ─────────
    public void PlayAndChangeGame()
    {
        if (isPlaying || _dragging) return;
        Play(() => GameSceneManager.Instance.OnChangeGame());
    }

    public void Play(System.Action onComplete)
    {
        if (tape == null || isPlaying) return;

        isPlaying = true;
        if (hoverEffectObject != null) hoverEffectObject.SetActive(true);

        seq?.Kill();
        tape.DOKill();
        tape.anchoredPosition = startPos;

        Vector2 backPos = startPos - forwardDir * backDistance;
        Vector2 overshootPos = insertedPos + forwardDir * overshoot;

        seq = DOTween.Sequence();
        seq.Append(tape.DOAnchorPos(backPos, backDuration).SetEase(Ease.OutQuad));      // 뒤로 살짝
        seq.Append(tape.DOAnchorPos(overshootPos, thrustDuration).SetEase(Ease.InQuad)); // 앞으로 팍
        AppendLockTail(onComplete);
        seq.Play();
    }

    // ───────── 슬라이드 경로 (현재 위치에서 이어서 삽입) ─────────
    private void PlayFromCurrent(System.Action onComplete)
    {
        if (tape == null || isPlaying) return;

        isPlaying = true;
        if (hoverEffectObject != null) hoverEffectObject.SetActive(true);

        seq?.Kill();
        tape.DOKill();

        Vector2 overshootPos = insertedPos + forwardDir * overshoot;

        seq = DOTween.Sequence();
        // 이미 손으로 앞쪽까지 밀었으니 back 단계는 생략, 현재 위치 → 오버슈트
        seq.Append(tape.DOAnchorPos(overshootPos, thrustDuration).SetEase(Ease.InQuad));
        AppendLockTail(onComplete);
        seq.Play();
    }

    // 체결 + 펀치 + 완료 처리 (두 경로 공용)
    private void AppendLockTail(System.Action onComplete)
    {
        seq.Append(tape.DOAnchorPos(insertedPos, lockDuration).SetEase(Ease.OutCubic));
        seq.Join(tape.DOPunchAnchorPos(-forwardDir * punch, punchDuration, 8, 0.6f)
            .OnComplete(() => GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.Cassette)));
        seq.OnComplete(() =>
        {
            isPlaying = false;
            tape.anchoredPosition = insertedPos;
            onComplete?.Invoke();
        });
    }

    private void SnapBack()
    {
        seq?.Kill();
        tape.DOKill();

        seq = DOTween.Sequence();
        seq.Append(tape.DOAnchorPos(startPos, snapBackDuration).SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            if (hoverEffectObject != null && !isPlaying) hoverEffectObject.SetActive(false);
        });
        seq.Play();
    }

    // ───────── 드래그 핸들러 ─────────
    public void OnBeginDrag(PointerEventData e)
    {
        if (isPlaying) { _dragging = false; return; }

        _dragging = true;
        seq?.Kill();
        tape.DOKill();
        if (hoverEffectObject != null) hoverEffectObject.SetActive(true);

        // 부모 로컬 좌표 기준으로 시작점 기록 (캔버스 스케일과 무관하게 정확)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, e.position, e.pressEventCamera, out _dragStartLocal);

        tape.anchoredPosition = startPos;
        _currentForward = 0f;
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_dragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, e.position, e.pressEventCamera, out Vector2 cur);

        // forwardDir 방향 성분만 추출 → 그 거리만큼만 따라옴
        float fwd = Vector2.Dot(cur - _dragStartLocal, forwardDir);
        fwd = Mathf.Clamp(fwd, 0f, _maxForward);

        _currentForward = fwd;
        tape.anchoredPosition = startPos + forwardDir * fwd;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!_dragging) return;
        _dragging = false;

        if (_currentForward >= _maxForward * slideThreshold)
            PlayFromCurrent(() => GameSceneManager.Instance.OnChangeGame());
        else
            SnapBack();
    }
}