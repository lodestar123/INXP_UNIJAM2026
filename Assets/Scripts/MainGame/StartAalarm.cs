using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartAalarm : MonoBehaviour, IPointerClickHandler
{
    [Header("자식 오브젝트 연결 (Inspector에서 할당)")]
    [SerializeField] private Image tutorialImage;   // 기존 "Image" 자식: 튜토리얼 삽화 표시용으로 재사용
    [SerializeField] private GameObject promptText; // 기존 "Text (TMP)" 자식: "터치해서 시작하기"
    [SerializeField] private Button startButton;    // 신규 추가: 튜토리얼 단계에서만 노출되는 버튼

    [Header("스테이지별 튜토리얼 이미지 (인덱스 = stageIndex)")]
    [SerializeField] private List<Sprite> tutorialSprites;

    // true: 이미지+버튼 단계(화면 클릭 무시). false: 텍스트 단계(화면 클릭 시 시작)
    private bool _isTutorialPhase;

    private void OnEnable()
    {
        int stageIndex = GameManager.Instance != null ? GameManager.Instance.currentStageNum : -1;
        bool isFirstPlay = IsFirstPlayOnStage(stageIndex);

        if (isFirstPlay && tutorialImage != null)
        {
            Sprite sprite = GetTutorialSprite(stageIndex);
            if (sprite != null)
            {
                tutorialImage.sprite = sprite;
            }
        }

        SetTutorialPhase(isFirstPlay);

        // 이 팝업이 떠 있는 동안(튜토리얼 단계든 "터치해서 시작하기" 단계든)은 게임 타이머를 멈춘다
        GameSceneManager.Instance?.SetInputGateActive(true);
    }

    private void SetTutorialPhase(bool showTutorial)
    {
        _isTutorialPhase = showTutorial;

        if (tutorialImage != null) tutorialImage.gameObject.SetActive(showTutorial);
        if (startButton != null) startButton.gameObject.SetActive(showTutorial);
        if (promptText != null) promptText.SetActive(!showTutorial);
    }

    private bool IsFirstPlayOnStage(int stageIndex)
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.GameData == null || gm.GameData.stageHighScore == null)
            return false;

        if (stageIndex < 0 || stageIndex >= gm.GameData.stageHighScore.Count)
            return false;

        return gm.GameData.stageHighScore[stageIndex] == -1;
    }

    private Sprite GetTutorialSprite(int stageIndex)
    {
        if (tutorialSprites == null || stageIndex < 0 || stageIndex >= tutorialSprites.Count)
            return null;

        return tutorialSprites[stageIndex];
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (_isTutorialPhase) return; // 튜토리얼 단계에서는 화면 클릭 완전 무시 (버튼으로만 다음 단계로)

        gameObject.SetActive(false); // 텍스트 단계에서 클릭하면 그때 게임 시작
        GameSceneManager.Instance?.SetInputGateActive(false);
    }

    // 버튼의 OnClick에 연결: 게임을 바로 시작하지 않고, 이미지+버튼을 끄고 텍스트 단계로 전환만 함
    public void OnTutorialStartButtonClicked()
    {
        SetTutorialPhase(false);
    }
}
