using UnityEngine;

// 애니팡(현재 게임)을 스테이지 0에서 처음 플레이할 때 딱 한 번 뜨는 일회성 팝업.
// 이미지/버튼은 프리팹에 이미 구성되어 있고, 이 스크립트는 패널 자체의 표시 여부만 담당한다.
public class PresentGameTutorialAlarm : MonoBehaviour
{
    private const int PresentTutorialStageIndex = 0;

    private void OnEnable()
    {
        if (!ShouldShowTutorial())
        {
            gameObject.SetActive(false);
            return;
        }

        // 팝업이 떠 있는 동안 게임 타이머를 멈춘다
        GameSceneManager.Instance?.SetInputGateActive(true);
    }

    private bool ShouldShowTutorial()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.GameData == null || gm.GameData.stageHighScore == null)
            return false;

        if (gm.currentStageNum != PresentTutorialStageIndex)
            return false;

        if (PresentTutorialStageIndex >= gm.GameData.stageHighScore.Count)
            return false;

        return gm.GameData.stageHighScore[PresentTutorialStageIndex] == -1;
    }

    // 버튼의 OnClick에 연결: 팝업을 닫는다.
    public void OnTutorialStartButtonClicked()
    {
        // 실제로 버튼을 눌러 확인했을 때만 "봤음"으로 기록 (stageHighScore -1 -> 0)
        GameManager.Instance?.MarkStageTutorialSeen(PresentTutorialStageIndex);

        gameObject.SetActive(false);
        GameSceneManager.Instance?.SetInputGateActive(false);
    }
}
