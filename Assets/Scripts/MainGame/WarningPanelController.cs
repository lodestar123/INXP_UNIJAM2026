using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class WarningPanelController : MonoBehaviour
{
    [Header("Panels")] // 연결 필요
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private GameObject warningImage1;
    [SerializeField] private GameObject warningImage2;
    [SerializeField] private float warningFadeDuration = 2.0f; // 페이드 전환 시간 (천천히)
    [SerializeField] private float warningDisplayDuration = 3.0f; // 각 이미지가 보이는 시간 (천천히) 

    private Sequence _warningAnimationSequence; // 경고 패널 애니메이션 시퀀스
    private void Start()
    {
        // GameSceneManager의 이벤트에 구독
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameChanged += OnGameChanged;
        }

        // ItemQueueManager의 경고 이벤트에 구독
        if (ItemQueueManager.Instance != null)
        {
            ItemQueueManager.Instance.OnWarningThresholdReached += OnWarningThresholdReached;
        }

    }

    private void OnDestroy()
    {
        StopWarningAnimation();

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameChanged -= OnGameChanged;
        }

        // ItemQueueManager 이벤트 구독 해제
        if (ItemQueueManager.Instance != null)
        {
            ItemQueueManager.Instance.OnWarningThresholdReached -= OnWarningThresholdReached;
        }
    }


    /// 아이템 큐에 아이템 49개 쌓이면 경고 패널 표시
    private void OnWarningThresholdReached()
    {
        if (warningPanel == null) return;

        warningPanel.SetActive(true);
        StartWarningAnimation();
    }

    // 경고 패널 이미지 반복 깜빡임 (DOTween 사용, 천천히)
    private void StartWarningAnimation()
    {
        StopWarningAnimation();

        if (warningImage1 == null || warningImage2 == null) return;

        // 각 GameObject에서 Image 컴포넌트 가져오기
        Image image1 = warningImage1.GetComponent<Image>();
        Image image2 = warningImage2.GetComponent<Image>();

        if (image1 == null || image2 == null) return;

        // 초기 상태 설정: image1 보이기, image2 숨기기
        image1.color = new Color(1f, 1f, 1f, 1f);
        image2.color = new Color(1f, 1f, 1f, 0f);

        // 두 GameObject 모두 활성화 (alpha로만 제어)
        warningImage1.SetActive(true);
        warningImage2.SetActive(true);

        // DOTween 시퀀스로 천천히 깜빡임
        _warningAnimationSequence = DOTween.Sequence();

        // image1이 보이는 상태에서 시작
        _warningAnimationSequence
            .Append(image1.DOFade(0f, warningFadeDuration)) // image1 천천히 페이드아웃
            .AppendInterval(warningDisplayDuration) // 딜레이
            .Append(image2.DOFade(1f, warningFadeDuration)) // image2 천천히 페이드인
            .AppendInterval(warningDisplayDuration) // image2 보이는 시간
            .Append(image2.DOFade(0f, warningFadeDuration)) // image2 천천히 페이드아웃
            .AppendInterval(warningDisplayDuration) // 딜레이
            .Append(image1.DOFade(1f, warningFadeDuration)) // image1 천천히 페이드인
            .AppendInterval(warningDisplayDuration) // image1 보이는 시간
            .SetLoops(-1); // 무한 반복
    }

    // 경고 패널 애니메이션 중지
    private void StopWarningAnimation()
    {
        // DOTween 시퀀스 중지
        if (_warningAnimationSequence != null)
        {
            _warningAnimationSequence.Kill();
            _warningAnimationSequence = null;
        }

        // Image 컴포넌트의 모든 애니메이션 중지 및 초기 상태 복원
        Image image1 = warningImage1?.GetComponent<Image>();

        if (image1 is not null)
        {
            image1.DOKill();
            image1.color = new Color(1f, 1f, 1f, 1f);
        }

        Image image2 = warningImage2?.GetComponent<Image>();

        if (image2 is null) return;

        image2.DOKill();
        image2.color = new Color(1f, 1f, 1f, 0f);
    }

    public void CloseWarningPanel()
    {
        StopWarningAnimation();
        warningPanel?.SetActive(false);
    }

    public void OnGameChanged()
    {
        CloseWarningPanel();
    }



}
