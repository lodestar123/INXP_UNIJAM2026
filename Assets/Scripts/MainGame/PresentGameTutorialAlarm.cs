using UnityEngine;
using UnityEngine.EventSystems;

// 애니팡(현재 게임)을 스테이지 0에서 처음 플레이할 때 딱 한 번 뜨는 일회성 팝업.
// 이미 본 뒤에는 팝업 자체가 뜨지 않고 바로 게임이 시작된다.
public class PresentGameTutorialAlarm : MonoBehaviour, IPointerClickHandler
{
    private const int PresentTutorialStageIndex = 0;

    private void OnEnable()
    {
        // 튜토리얼을 이미 본 뒤에는 대기 없이 곧바로 시작한다 (팝업 자체를 띄우지 않음)
        if (!ShouldShowTutorial())
        {
            gameObject.SetActive(false);
            return;
        }

        // 팝업이 떠 있는 동안 게임 타이머를 멈춘다
        GameSceneManager.Instance?.SetInputGateActive(true);
    }

    private static bool ShouldShowTutorial()
    {
        GameManager gm = GameManager.Instance;
        return gm != null &&
               gm.currentStageNum == PresentTutorialStageIndex &&
               gm.GameData != null &&
               gm.GameData.stageHighScore != null &&
               PresentTutorialStageIndex < gm.GameData.stageHighScore.Count &&
               gm.GameData.stageHighScore[PresentTutorialStageIndex] == -1;
    }

    // 튜토리얼 단계에서는 화면 클릭을 무시한다 (버튼으로만 닫힘)
    public void OnPointerClick(PointerEventData eventData)
    {
    }

    // 버튼의 OnClick에 연결: 튜토리얼을 끝낸다.
    public void OnTutorialStartButtonClicked()
    {
        GameManager.Instance?.MarkStageTutorialSeen(PresentTutorialStageIndex);
        CloseTutorial();
    }

    private void CloseTutorial()
    {
        gameObject.SetActive(false);
        GameSceneManager.Instance?.SetInputGateActive(false);
    }
}
