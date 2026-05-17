using UnityEngine;
using UnityEngine.UI;

public class ReViewManager : MonoBehaviour
{
    [SerializeField] private GameObject settigPanel; // 설정 패널 참조

    [Header("Scene Names")]
    [SerializeField] private string LobbySceneName = "LobbyScene";
    [SerializeField] private string CutSceneName = "CutScene";
    [SerializeField] private string ReViewSceneName = "ReViewScene";

    //[SerializeField] private Image cutSceneImage; // 컷씬 이미지 참조

    void Start()
    {
        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Title);
        settigPanel.SetActive(false); // 설정 패널 초기화 - 숨김

        // 이미지 초기화 - 각 스테이지 컷씬 해금 여부에 따라 이미지 변경
        // cutSceneImage.sprite = null;
    }

    public void onGoToCutScene(int stageIndex)
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        if (GameManager.Instance.GameData.stageUnlocked[stageIndex + 1])
        {
            GameManager.Instance.currentStageNum = stageIndex; // 컷씬에서 보여줄 스테이지 번호 지정
            GameManager.Instance.nextSceneAfterCutscene = ReViewSceneName; // 컷씬 이후 다시보기 씬으로 이동해야 함 지정

            SceneLoader.Load(CutSceneName);
        }
        else
        {

            Debug.Log("해금되지 않은 스테이지입니다.");
        }
    }

    public void onGoToLobby()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        SceneLoader.Load(LobbySceneName);
    }

    public void onSettingButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        settigPanel.SetActive(!settigPanel.activeSelf);
    }
}
