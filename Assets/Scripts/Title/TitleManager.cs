using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Collections;
public class TitleManager : MonoBehaviour
{
    [Header("Panels")] // 연결 필요
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject dashboardPanel;
    [Header("Text")]
    public TextMeshProUGUI dashboardText;


    [Header("Scene Names")]
    [SerializeField] private string lobbySceneName = "LobbyScene";

    [Header("Prologue Manager")]
    [SerializeField] private PrologueManager prologueManager;

    private bool isWaitingForPrologue = false;

    private void Start()
    {
        settingPanel.SetActive(false);
        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Title);
    }

    public void OnStartGameButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

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
    }

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

    /// <summary>
    /// 게임 시작
    /// </summary>
    private void StartGame()
    {
        SceneManager.LoadScene(lobbySceneName);
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
}
