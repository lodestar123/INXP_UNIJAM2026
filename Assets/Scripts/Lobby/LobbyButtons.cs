using UnityEngine;
using Utils;

public class LobbyButtons : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string GameSceneName = "MainScene";

    public void LoadStage(int stageIndex)
    {
        // GameManager의 currentStageNum을 해당하는 번호로 설정, 게임 씬으로 이동
        if (GameManager.Instance.GameData.stageUnlocked[stageIndex])
        {
            GameManager.Instance.currentStageNum = stageIndex;
            SceneLoader.Load(GameSceneName);
        }
        else
        {
            CustomLog.Info("해금되지 않은 스테이지입니다.");
        }
    }
}
