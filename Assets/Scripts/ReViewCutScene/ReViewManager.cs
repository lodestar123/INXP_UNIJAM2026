using UnityEngine;
using UnityEngine.UI;

public class ReViewManager : MonoBehaviour
{
    [SerializeField] private GameObject settigPanel; // 설정 패널 참조

    [Header("Scene Names")]
    [SerializeField] private string LobbySceneName = "LobbyScene";
    [SerializeField] private string CutSceneName = "CutScene";
    [SerializeField] private string ReViewSceneName = "ReViewScene";

    [Header("Cutscene Buttons")]
    [SerializeField] private Button[] cutSceneButtons;   // 버튼 6개

    void Start()
    {
        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Title);
        settigPanel.SetActive(false); // 설정 패널 초기화 - 숨김

        InitializeCutSceneButtons();
    }
    private void InitializeCutSceneButtons()
    {
        for (int i = 0; i < cutSceneButtons.Length; i++)
        {
            bool isUnlocked = IsButtonUnlocked(i);

            // 스프라이트는 항상 동일하게 유지하고, 잠금 여부는 Button의
            // interactable 상태(Color Tint의 Disabled Color)로만 표현한다.
            cutSceneButtons[i].interactable = isUnlocked;
        }
    }

    private bool IsButtonUnlocked(int buttonIndex)
    {
        var gameData = GameManager.Instance.GameData;

        switch (buttonIndex)
        {
            case 0:
                return true; // 인트로

            case 1:
            case 2:
            case 3:
            case 4:
                // 이전 스테이지 하이스코어가 클리어 기준 이상이면 언락
                int scoreIndex = buttonIndex - 1;
                return gameData.stageHighScore[scoreIndex] >= gameData.stageClearCriteria[scoreIndex];

            case 5:
                // 막스테 클리어시 자동 언락
                return IsButtonUnlocked(4);

            default:
                return false;
        }
    }

    public void onGoToCutScene(int stageIndex)
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        if (IsButtonUnlocked(stageIndex + 1))
        {
            GameManager.Instance.currentStageNum = stageIndex;
            GameManager.Instance.nextSceneAfterCutscene = ReViewSceneName;
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
