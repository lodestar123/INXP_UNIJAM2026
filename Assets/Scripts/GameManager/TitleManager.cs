using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("Panels")] // 연결 필요
    [SerializeField] private GameObject settingPanel;


    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "MainScene";

    private void Start()
    {
        settingPanel.SetActive(false);
        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Title);
    }

    public void OnStartGameButton() // 게임 시작 버튼 클릭
    {

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        SceneManager.LoadScene(gameSceneName);
    }
    public void OnSettingButton() // 설정 버튼 클릭
    {

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        settingPanel.SetActive(true);
    }
    public void OnCloseSettingButton() // 설정 닫기 버튼 클릭
    {

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        settingPanel.SetActive(false);
    }
    public void OnQuitButton() // 종료 버튼 클릭
    {

        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        Application.Quit();
    }
}
