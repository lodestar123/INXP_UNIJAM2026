using UnityEngine;

public class LobbyButtons : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string CutSceneName = "CutScene";
    [SerializeField] private string GameSceneName = "MainScene";

    public void LoadStage(int stageIndex)
    {
        // GameManager의 currentStageNum을 해당하는 번호로 설정, 컷씬으로 이동
        if (GameManager.Instance.GameData.stageUnlocked[stageIndex])
        {
            GameManager.Instance.currentStageNum = stageIndex;
            GameManager.Instance.nextSceneAfterCutscene = GameSceneName; // 컷씬 이후 게임 씬으로 이동해야 함 지정
            SceneLoader.Load(CutSceneName);
        }
        else
        {
            Debug.Log("해금되지 않은 스테이지입니다.");
        }
    }
}
