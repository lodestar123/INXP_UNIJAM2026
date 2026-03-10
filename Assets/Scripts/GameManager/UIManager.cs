using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{

    [Header("Panels")] // 연결 필요
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject gameOverPanel; // 게임 오버 패널


    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "Title";

    [SerializeField] private string gameSceneName = "MainScene";



    private bool isGameChanging = false; // 게임 전환 중인지 여부
    private bool isRecorded = false; // 저장 여부

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
        }

        isRecorded = false; // 저장 초기화
    }

    private void OnDestroy()
    {

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameOver -= OnGameOver;

        }

    }
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameSceneManager.Instance is not null && GameSceneManager.Instance.IsTransitioning) return;

            HandleBackAction(); // 뒤로가기
        }
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

        //CloseWarningPanel();

        //OnGameOverTextUpdate(); // 텍스트 업데이트 + 스테이지 해금

        GameManager.Instance.UpdateStageHighScore(GameSceneManager.Instance.CurrentScore); // 스테이지별 최고점수 기록 업데이트

    }

    public void OnChangeGameButton() // 게임 전환 버튼 클릭
    {
        if (state != PauseUIState.Closed) return; // 게임 진행 중일 때만 전환 허용
        if (isGameChanging) return; // 이미 전환 중이면 무시

        isGameChanging = true;

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        GameSceneManager.Instance.OnChangeGame();

        isGameChanging = false;
    }
    public void OnPauseGame() // 퍼즈 버튼 클릭
    {
        if (GameSceneManager.Instance is not null && GameSceneManager.Instance.IsTransitioning) return;
        if (GameSceneManager.Instance.IsGameOver) return; // 게임오버 시 무시

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        ApplyState(PauseUIState.PauseMenu); // 퍼즈 메뉴 상태로 전환
    }

    public void OnResumeGame() // 재개 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        ApplyState(PauseUIState.Closed);
    }
    public void OnRestartGame() // 재시작 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        ApplyState(PauseUIState.Closed);
        SceneManager.LoadScene(gameSceneName); // 게임 씬 다시 로드
    }

    public void OnQuitGame() // 게임 종료 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        ApplyState(PauseUIState.Closed);
        GameManager.Instance.currentStageNum = -1; // 스테이지 밖으로 설정
        SceneManager.LoadScene(titleSceneName); // 타이틀 씬으로
    }

    public void OpenSettingPanel() // 설정 버튼 클릭
    {
        if (GameSceneManager.Instance.IsGameOver) return; // 게임오버 시 무시

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        if (state != PauseUIState.PauseMenu) return; // 퍼즈 메뉴일 때만 설정으로 진입 허용
        ApplyState(PauseUIState.Settings);
    }

    public void CloseSettingPanel() // 설정 닫기 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        ApplyState(PauseUIState.PauseMenu);
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

    }

}
