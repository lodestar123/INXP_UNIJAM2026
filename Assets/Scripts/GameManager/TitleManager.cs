using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
public class TitleManager : MonoBehaviour
{
    [Header("Panels")] // 연결 필요
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject dashboardPanel;
    [Header("Text")]
    public TextMeshProUGUI dashboardText;


    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "MainScene";

    private void Start()
    {
        settingPanel.SetActive(false);
        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Title);
    }

    public void OnStartGameButton() // 게임 시작 버튼 클릭
    {

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

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
            dashboardContent += rank + ". [ " + score.Key + " ]  " + score.Value + " 점\n";
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
