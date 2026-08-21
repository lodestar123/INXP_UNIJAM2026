using UnityEngine;
using UnityEngine.EventSystems;

// 애니팡(현재 게임)을 스테이지 0에서 처음 플레이할 때 딱 한 번 뜨는 일회성 팝업.
// 첫 플레이 이후에는 같은 패널을 현재 게임 시작 대기용으로 재사용한다.
public class PresentGameTutorialAlarm : MonoBehaviour, IPointerClickHandler
{
    private const int PresentTutorialStageIndex = 0;

    private bool _isTutorialPhase;

    private void OnEnable()
    {
        if (!ShouldUsePresentStartGate())
        {
            gameObject.SetActive(false);
            return;
        }

        _isTutorialPhase = ShouldShowTutorial();

        // 팝업이 떠 있는 동안 게임 타이머를 멈춘다
        GameSceneManager.Instance?.SetInputGateActive(true);
    }

    private static bool ShouldUsePresentStartGate()
    {
        GameManager gm = GameManager.Instance;
        return gm != null && gm.currentStageNum == PresentTutorialStageIndex;
    }

    private static bool ShouldShowTutorial()
    {
        GameManager gm = GameManager.Instance;
        return gm != null &&
               gm.GameData != null &&
               gm.GameData.stageHighScore != null &&
               PresentTutorialStageIndex < gm.GameData.stageHighScore.Count &&
               gm.GameData.stageHighScore[PresentTutorialStageIndex] == -1;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isTutorialPhase)
        {
            return;
        }

        CloseStartGate();
    }

    // 버튼의 OnClick에 연결: 튜토리얼/시작 대기를 끝낸다.
    public void OnTutorialStartButtonClicked()
    {
        // 실제로 버튼을 눌러 확인했을 때만 "봤음"으로 기록 (stageHighScore -1 -> 0)
        if (_isTutorialPhase)
        {
            GameManager.Instance?.MarkStageTutorialSeen(PresentTutorialStageIndex);
        }

        CloseStartGate();
    }

    private void CloseStartGate()
    {
        gameObject.SetActive(false);
        GameSceneManager.Instance?.SetInputGateActive(false);
    }
}
