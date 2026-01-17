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
    [SerializeField] private string gameSceneName = "MainScene";

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
        SceneManager.LoadScene(gameSceneName);
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
        dashboardText.text = dashboardContent.TrimEnd(); // 마지막 개행 제거

        dashboardPanel.SetActive(true);
    }

    public void OnCloseDashboardButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        dashboardPanel.SetActive(false);
    }
    public void OnQuitButton() // 종료 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생
        Application.Quit();
    }
}
