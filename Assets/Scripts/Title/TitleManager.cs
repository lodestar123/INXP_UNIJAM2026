using System.Collections;
using UnityEngine;
using TMPro;
using System.Linq;
using BackEnd;

public class TitleManager : MonoBehaviour
{
    [Header("Panels")] // 연결 필요
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject dashboardPanel;
    [SerializeField] private GameObject writeNamePanel;
    [SerializeField] private TMP_InputField inputName;

    [Header("Title flow")]
    public GameObject mainButtons;
    public GameObject loginButtons;

    [Header("Text")]
    public TextMeshProUGUI dashboardText;


    [Header("Scene Names")]
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string CutSceneName = "CutScene";

    [Header("Prologue Manager")]
    [SerializeField] private PrologueManager prologueManager;

    private const string PrefKeyNicknameSetupCompleted = "TitleNicknameSetupCompleted";

    private bool _pendingOpenWriteNamePanel;
    private bool _pendingShowMainAfterLogin;
    private bool _pendingApplyUiAfterGoogleSignOut;

    private void Update()
    {
        if (_pendingOpenWriteNamePanel)
        {
            _pendingOpenWriteNamePanel = false;
            OpenWriteNamePanel();
        }

        if (_pendingShowMainAfterLogin)
        {
            _pendingShowMainAfterLogin = false;
            ApplyTitleUiLoggedIn();
        }

        if (_pendingApplyUiAfterGoogleSignOut)
        {
            _pendingApplyUiAfterGoogleSignOut = false;
            ApplyUiAfterGoogleSignOut();
        }
    }

    private void Start()
    {
        settingPanel.SetActive(false);
        if (writeNamePanel != null)
            writeNamePanel.SetActive(false);
        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Title);
        GameManager.Instance.currentStageNum = -1; // 타이틀 진입 시 스테이지 번호 초기화

        StartCoroutine(CoApplyInitialTitleUi());
    }

    private IEnumerator CoApplyInitialTitleUi()
    {
        yield return null;
        ApplyInitialTitleUi();
    }

    private void ApplyInitialTitleUi()
    {
        if (!BackendLogin.IsLoggedIn())
        {
            if (mainButtons != null)
                mainButtons.SetActive(false);
            if (loginButtons != null)
                loginButtons.SetActive(true);
            return;
        }

        if (!IsNicknameSetupCompleted())
        {
            if (mainButtons != null)
                mainButtons.SetActive(false);
            if (loginButtons != null)
                loginButtons.SetActive(false);
            OpenWriteNamePanel();
            return;
        }

        ApplyTitleUiLoggedIn();
    }

    private void OpenWriteNamePanel()
    {
        if (writeNamePanel == null)
        {
            Debug.LogWarning("TitleManager: writeNamePanel이 인스펙터에 연결되지 않았습니다.");
            return;
        }

        writeNamePanel.SetActive(true);
        if (inputName != null)
        {
            inputName.text = string.Empty;
            inputName.Select();
            inputName.ActivateInputField();
        }
    }

    public void OnStartGameButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        if (GameManager.Instance.GameData.stageHighScore[0] != -1) // 최초 플레이 감지
        {
            StartGame();
        }
        else
        {
            // 최초 플레이 인트로 컷씬 재생
            GameManager.Instance.nextSceneAfterCutscene = lobbySceneName; // 컷씬 이후 로비로 이동
            SceneLoader.Load(CutSceneName); // 컷씬 씬으로 이동
        }


        /*
                // 프롤로그 최초 1회 표시
                if (prologueManager == null)
                {
                    StartGame();
                    return;
                }

                bool shouldShowPrologue = prologueManager.ShowPrologueIfNeeded();

                if (shouldShowPrologue)
                {
                    isWaitingForPrologue = true;

                    // 프롤로그 완료 콜백 설정
                    prologueManager.SetOnCompletedCallback(() =>
                    {
                        isWaitingForPrologue = false;
                        StartGame();
                    });

                    // 프롤로그 패널이 비활성화될 때까지 대기
                    StartCoroutine(WaitForPrologueComplete());
                }
                else
                {
                    StartGame();
                }
                */ // 구버전 프롤로그 주석처리
    }
    /*
        /// <summary>
        /// 프롤로그 완료를 기다리는 코루틴
        /// </summary>
        private IEnumerator WaitForPrologueComplete()
        {
            // 프롤로그 패널이 비활성화될 때까지 대기
            while (isWaitingForPrologue)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    */
    /// <summary>
    /// 게임 시작
    /// </summary>
    private void StartGame()
    {
        SceneLoader.Load(lobbySceneName);
    }

    public void OnSettingButton() // 설정 버튼 클릭
    {

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        settingPanel.SetActive(true);
    }

    public void OnCloseSettingButton() // 설정 닫기 버튼 클릭
    {

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        settingPanel.SetActive(false);
    }
    public void OnDashboardButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        if (dashboardPanel == null)
        {
            Debug.LogWarning("TitleManager: dashboardPanel이 인스펙터에 할당되지 않았습니다. 대시보드 패널 오브젝트를 연결해주세요.");
            return;
        }
        dashboardPanel.SetActive(true);
    }

    /// <summary>
    /// 로컬 highScores로 대시보드 텍스트를 채웁니다.
    /// </summary>
    private void ShowLocalDashboard()
    {
        if (dashboardPanel == null || dashboardText == null) return;

        if (GameManager.Instance.highScores == null || GameManager.Instance.highScores.Count == 0)
        {
            // 게임 데이터 로드 시도
            SaveLoadManager.Instance.LoadGame();

            // 로드 후에도 비어있는지 다시 확인
            if (GameManager.Instance.highScores == null || GameManager.Instance.highScores.Count == 0)
            {
                dashboardText.text = "\n아직 아무도 플레이하지 않았어요.\t\t\t";
                dashboardPanel.SetActive(true);
                return;
            }
        }
        // highScores를 점수 기준으로 내림차순 정렬
        var sortedScores = GameManager.Instance.highScores
            .OrderByDescending(x => x.Value)
            .ToList();

        // dashboardText에 대입
        string dashboardContent = "";
        int rank = 1;

        foreach (var score in sortedScores)
        {
            int dashLength = 20 - score.Key.Length - score.Value.ToString().Length;
            dashboardContent += rank + ". [ " + score.Key + " ] " + new string('-', dashLength) + " " + score.Value + " 점\n";
            rank++;
        }
        dashboardText.text = dashboardContent.TrimEnd();
    }

    public void OnCloseDashboardButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        if (dashboardPanel != null)
            dashboardPanel.SetActive(false);
    }
    public void OnQuitButton() // 종료 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생
        Application.Quit();
    }

    public void OnGoogleLoginButton() //구글 로그인 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        TheBackend.ToolKit.GoogleLogin.Android.GoogleLogin(GoogleLoginCallback);
    }

    private void GoogleLoginCallback(bool isSuccess, string errorMessage, string token) // 구글 로그인 콜백
    {
        if (isSuccess == false)
        {
            Debug.LogError(errorMessage);
            return;
        }

        Debug.Log("구글 토큰 : " + token);
        var bro = Backend.BMember.AuthorizeFederation(token, FederationType.Google);
        Debug.Log("페데레이션 로그인 결과 : " + bro);

        if (!bro.IsSuccess())
        {
            Debug.LogError("뒤끝 구글 페더레이션 로그인 실패: " + bro);
            return;
        }

        BackendGameData.Instance.EnsureUserDataForCurrentUser();

        if (IsNicknameSetupCompleted())
        {
            _pendingShowMainAfterLogin = true;
        }
        else
        {
            _pendingOpenWriteNamePanel = true;
        }
    }
    
    // WriteNamePanel에서 이름 확정 시: 입력값을 뒤끝 로그인 계정 닉네임으로 반영
    public void OnWriteNameConfirmButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        string nickname = inputName.text == null ? string.Empty : inputName.text.Trim();
        if (string.IsNullOrEmpty(nickname)) return;
        
        //뒤끝 닉네임 저장
        var bro = BackendLogin.Instance.UpdateNickname(nickname);
        if (!bro.IsSuccess()) return;

        GameManager.Instance.GameData.playerName = nickname;
        SaveLoadManager.Instance?.SaveGame();

        MarkNicknameSetupCompleted();
        ApplyTitleUiLoggedIn();
    }

    public void OnTestLoginButton(){
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        BackendLogin.Instance.CustomLogin("user1", "1234");
        if (IsNicknameSetupCompleted())
        {
            _pendingShowMainAfterLogin = true;
        }
        else
        {
            _pendingOpenWriteNamePanel = true;
        }
    }

    public void SignOutGoogleLogin()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        TheBackend.ToolKit.GoogleLogin.Android.GoogleSignOut(GoogleSignOutCallback);
    }

    private void GoogleSignOutCallback(bool isSuccess, string error)
    {
        if (isSuccess == false)
        {
            Debug.Log("구글 로그아웃 에러 응답 발생 : " + error);
            return;
        }

        Debug.Log("구글 로그아웃 성공");
        _pendingApplyUiAfterGoogleSignOut = true;
    }

    private void ApplyUiAfterGoogleSignOut()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (mainButtons != null)
            mainButtons.SetActive(false);

        if (loginButtons != null)
            loginButtons.SetActive(true);
    }

    private static bool IsNicknameSetupCompleted()
    {
        return PlayerPrefs.GetInt(PrefKeyNicknameSetupCompleted, 0) == 1;
    }

    private static void MarkNicknameSetupCompleted()
    {
        PlayerPrefs.SetInt(PrefKeyNicknameSetupCompleted, 1);
        PlayerPrefs.Save();
    }

    private void ApplyTitleUiLoggedIn()
    {
        if (writeNamePanel != null)
            writeNamePanel.SetActive(false);

        if (mainButtons != null)
            mainButtons.SetActive(true);

        if (loginButtons != null)
            loginButtons.SetActive(false);
    }
}
