using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [Header("Panels")] // 연결 필요
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private GameObject warningImage1;
    [SerializeField] private GameObject warningImage2;
    [SerializeField] private float warningFadeDuration = 2.0f; // 페이드 전환 시간 (천천히)
    [SerializeField] private float warningDisplayDuration = 3.0f; // 각 이미지가 보이는 시간 (천천히) 

    [Header("Game Object")]
    // public GameObject pauseButtons; // 퍼즈화면 버튼들 부모
    public GameObject gameOverPanel; // 게임 오버 패널
    public TextMeshProUGUI gameResult; // 게임 결과 출력
    public TextMeshProUGUI alarm; // 기록 저장 여부 등 출력
    public TMP_InputField inputName; // 입력한 이름

    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "Title";

    [SerializeField] private string gameSceneName = "MainScene";

    [Header("UI Style")]
    [SerializeField] private UiSpriteChanger uiSpriteChanger; // UiSpriteChanger 참조 추가


    private bool isGameChanging = false; // 게임 전환 중인지 여부
    private Sequence _warningAnimationSequence; // 경고 패널 애니메이션 시퀀스


    private enum PauseUIState // 퍼즈 UI 상태
    {
        Closed, // 게임 진행 중
        PauseMenu, // 퍼즈 메뉴만 열림
        Settings, // 퍼즈 + 설정 열림
        GameOver, // 게임오버 열림
    }

    private PauseUIState state = PauseUIState.Closed; // 현재 상태

    private void Awake()
    {
        isGameChanging = false;
        ApplyState(PauseUIState.Closed); // 시작은 닫힘으로 강제
    }
    private void Start()
    {
        // GameSceneManager의 이벤트에 구독
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameOver += OnGameOver;
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
            GameSceneManager.Instance.OnGameOver -= OnGameOver;
            GameSceneManager.Instance.OnGameChanged -= OnGameChanged;
        }

        // ItemQueueManager 이벤트 구독 해제
        if (ItemQueueManager.Instance != null)
        {
            ItemQueueManager.Instance.OnWarningThresholdReached -= OnWarningThresholdReached;
        }
    }
    private void Update()
    {
        /*
            if (Input.GetKeyDown(KeyCode.Escape)) // 뉴인풋 사용하면 수정 필요할듯?
            {
                HandleBackAction(); // 뒤로가기
            }
            */
    }

    private void HandleBackAction() // 모바일 뒤로가기 버튼
    {
        if (state == PauseUIState.Settings) // 설정이 열려 있으면 설정 닫음
        {
            ApplyState(PauseUIState.PauseMenu);
            return;
        }

        if (state == PauseUIState.PauseMenu) // 퍼즈만 열려 있으면 퍼즈 닫음
        {
            ApplyState(PauseUIState.Closed);
            return;
        }

        ApplyState(PauseUIState.PauseMenu); // Closed 일 시 퍼즈 열기
    }
    void OnGameOver()
    {
        ApplyState(PauseUIState.GameOver);

        gameResult.text = $"점수 : {GameSceneManager.Instance.CurrentScore} 점";


        // 최종 점수 비교 전달
        if (GameManager.Instance != null && GameManager.Instance.GameData != null)
        {
            if (GameManager.Instance.highScores.Count > 0)
            {
                int maxScore = GameManager.Instance.highScores.Values.Count > 0
                    ? System.Linq.Enumerable.Max(GameManager.Instance.highScores.Values)
                    : 0;

                if (GameSceneManager.Instance.CurrentScore > maxScore)
                {
                    alarm.text = "신기록!";
                }
            }
            else if (GameSceneManager.Instance.CurrentScore > 0)
            {
                alarm.text = "신기록!";
            }
        }

    }

    public void RecordScore()
    {
        try
        {
            GameManager.Instance.highScores.Add(inputName.text, GameSceneManager.Instance.CurrentScore);
            alarm.text = "기록이 저장되었습니다!";
        }
        catch
        {
            GameManager.Instance.highScores[inputName.text] = GameSceneManager.Instance.CurrentScore;
            alarm.text = "새로운 기록으로 교체되었습니다!";
        }

    }

    public void OnChangeGameButton() // 게임 전환 버튼 클릭
    {
        if (state != PauseUIState.Closed) return; // 게임 진행 중일 때만 전환 허용
        if (isGameChanging) return; // 이미 전환 중이면 무시

        isGameChanging = true;

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        // 지지직 연출 생성으로 화면 가리기
        GameSceneManager.Instance.OnChangeGame();
        // 연출 삭제되는 연출의 연출 연출...

        isGameChanging = false;
    }
    public void OnPauseGame() // 퍼즈 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        ApplyState(PauseUIState.PauseMenu); // 퍼즈 메뉴 상태로 전환
    }

    public void OnResumeGame() // 재개 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        ApplyState(PauseUIState.Closed);
    }
    public void OnRestartGame() // 재시작 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        ApplyState(PauseUIState.Closed);
        SceneManager.LoadScene(gameSceneName); // 게임 씬 다시 로드
    }

    public void OnQuitGame() // 게임 종료 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        ApplyState(PauseUIState.Closed);
        SceneManager.LoadScene(titleSceneName); // 타이틀 씬으로
    }

    public void OpenSettingPanel() // 설정 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        if (state != PauseUIState.PauseMenu) return; // 퍼즈 메뉴일 때만 설정으로 진입 허용
        ApplyState(PauseUIState.Settings);
    }

    public void CloseSettingPanel() // 설정 닫기 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        ApplyState(PauseUIState.PauseMenu);
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

    private void ApplyState(PauseUIState newState) // 매개변수로 현 상태 받음
    {
        if (state == newState) return; // 동일 상태면 무시
        if (isGameChanging) return; // 게임 전환 중이면 무시

        state = newState;

        bool isPaused = (state != PauseUIState.Closed); // 퍼즈 여부 계산

        if (GameSceneManager.Instance is not null)
        {
            GameSceneManager.Instance.OnApplicationPause(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;

        pausePanel?.SetActive(state == PauseUIState.PauseMenu);
        settingPanel?.SetActive(state == PauseUIState.Settings);
        gameOverPanel?.SetActive(state == PauseUIState.GameOver);
        
        // UI 스타일 적용 (UiSpriteChanger가 자체적으로 실행)
        uiSpriteChanger?.ApplyUiStyle();

        // pauseButtons.SetActive(state != PauseUIState.Settings);

    }

}
