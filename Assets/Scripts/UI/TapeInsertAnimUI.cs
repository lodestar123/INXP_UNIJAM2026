using UnityEngine; // 유니티 기본
using UnityEngine.UI; // UI
using DG.Tweening; // DOTween
using UnityEngine.EventSystems;

public class TapeInsertAnimUI : MonoBehaviour // UI 테이프 삽입 연출
{
    [Header("Refs")] // 참조
    [SerializeField] private RectTransform tape; // 움직일 UI
    [SerializeField] private Button clickButton; // 버튼(선택)
    [SerializeField] private GameObject hoverEffectObject; // 마우스 오버 효과 오브젝트

    [Header("Positions (Anchored)")] // 앵커 기준 좌표
    [SerializeField] private Vector2 startPos; // 시작 위치
    [SerializeField] private Vector2 insertedPos; // 최종 체결 위치

    [Header("Direction")] // 뒤로/앞 방향(화면 기준)
    [SerializeField] private Vector2 forwardDir = Vector2.up; // "앞으로" 방향(기본 위쪽). 필요하면 (1,0) 등으로 변경

    [Header("Timing")] // 시간
    [SerializeField] private float backDuration = 0.08f; // 뒤로 살짝
    [SerializeField] private float thrustDuration = 0.18f; // 앞으로 팍
    [SerializeField] private float lockDuration = 0.10f; // 체결 감속

    [Header("Tuning")] // 연출 강도
    [SerializeField] private float backDistance = 30f; // 뒤로 빠지는 픽셀
    [SerializeField] private float overshoot = 8f; // 오버슈트 픽셀
    [SerializeField] private float punch = 6f; // 탁 진동 픽셀
    [SerializeField] private float punchDuration = 0.08f; // 진동 시간

    private Sequence seq; // 시퀀스
    private bool isPlaying; // 중복 방지
    private RectTransform rect; // 캐시용 RectTransform


    private void Awake() // 초기화
    {
        if (tape == null) tape = GetComponent<RectTransform>(); // 없으면 자기 자신
        startPos = tape.anchoredPosition; // 현재를 시작으로
        forwardDir = forwardDir.sqrMagnitude < 0.0001f ? Vector2.up : forwardDir.normalized; // 0벡터 방지+정규화

        if (clickButton != null) // 버튼이 있으면
        {
            clickButton.onClick.AddListener(PlayAndChangeGame); // 클릭 연결

            // 마우스 오버 이벤트 동적 연결
            EventTrigger trigger = clickButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = clickButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => { if (hoverEffectObject != null) hoverEffectObject.SetActive(true); });
            trigger.triggers.Add(entryEnter);

            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => { if (hoverEffectObject != null) hoverEffectObject.SetActive(false); });
            trigger.triggers.Add(entryExit);
        }

        if (hoverEffectObject != null) hoverEffectObject.SetActive(false); // 초기화 시 비활성화


        rect = GetComponent<RectTransform>(); // RectTransform 캐싱
        startPos = rect.anchoredPosition; // 최초 위치를 시작 위치로 저장
    }
    void OnEnable()
    {
        rect.DOKill();                 // 남아있는 트윈 제거
        rect.anchoredPosition = startPos; // 위치 초기화
    }
    public void PlayAndChangeGame() // 버튼에서 호출할 함수
    {
        if (isPlaying) return; // 중복 방지
        Play(() => GameSceneManager.Instance.OnChangeGame()); // 끝나면 씬 전환 호출
    }

    public void Play(System.Action onComplete) // 외부 콜백 받는 재사용 함수
    {
        if (tape == null) return; // 방어
        if (isPlaying) return; // 중복 방지

        isPlaying = true; // 재생 시작

        seq?.Kill(); // 기존 시퀀스 정리
        tape.DOKill(); // 트윈 겹침 방지

        tape.anchoredPosition = startPos; // 시작 위치 고정

        Vector2 backPos = startPos - forwardDir * backDistance; // 뒤로 살짝
        Vector2 overshootPos = insertedPos + forwardDir * overshoot; // 앞으로 팍 + 오버슈트

        seq = DOTween.Sequence(); // 생성

        seq.Append( // 1) 뒤로 살짝
            tape.DOAnchorPos(backPos, backDuration)
                .SetEase(Ease.OutQuad)
        );

        seq.Append( // 2) 앞으로 팍
            tape.DOAnchorPos(overshootPos, thrustDuration)
                .SetEase(Ease.InQuad)
        );

        seq.Append( // 3) 체결: 감속하며 자리 잡기
            tape.DOAnchorPos(insertedPos, lockDuration)
                .SetEase(Ease.OutCubic)
        );

        seq.Join( // 4) 탁: 미세 진동
            tape.DOPunchAnchorPos(-forwardDir * punch, punchDuration, 8, 0.6f)
            .OnComplete(() => GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.Cassette))
        );

        seq.OnComplete(() => // 완료 처리
        {
            isPlaying = false; // 재생 종료
            tape.anchoredPosition = insertedPos; // 최종 고정
            onComplete?.Invoke(); // 콜백 실행
        });

        seq.Play(); // 재생
    }
}
