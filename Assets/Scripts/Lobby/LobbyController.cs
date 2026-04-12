using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LobbyController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject stageSelectPanel; // 스테이지 선택 패널

    // [SerializeField] private TMPro.TextMeshProUGUI stageNameText; // 스테이지 이름 텍스트
    [SerializeField] private TMPro.TextMeshProUGUI stageCriteriaText; // 스테이지 달성 기준 텍스트
    [SerializeField] private TMPro.TextMeshProUGUI stageDescriptionText; // 스테이지 설명 텍스트
    // [SerializeField] private Button stageSelectButton; // 스테이지 선택 버튼

    [Header("Stage Data")]
    [SerializeField] private List<LobbyStageData> stageDatas; // 스테이지 데이터 배열
    [Header("Scene Names")]
    [SerializeField] private string titleSceneName = "Title";
    [SerializeField] private string gameSceneName = "MainScene";

    private int nowStageIndex = 0;

    private void Start()
    {
        stageSelectPanel.SetActive(false);
    }
    public void openStageSelectPanel(int stageIndex)
    {
        nowStageIndex = stageIndex;
        stageSelectPanel.SetActive(true); // 스테이지 선택 패널 활성화

        //stageNameText.text = stageDatas[stageIndex].StageName; // 스테이지 이름 표시
        stageCriteriaText.text = stageDatas[stageIndex].normalStageCriteria; // 스테이지 달성 기준 표시
        stageDescriptionText.text = stageDatas[stageIndex].stageDescription; // 스테이지 설명 표시
    }
    public void onStartStageButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        if (GameManager.Instance.GameData.stageUnlocked[nowStageIndex])
        {
            GameManager.Instance.currentStageNum = nowStageIndex;
            SceneLoader.Load(gameSceneName);
        }
        else
        {
            Debug.Log("해금되지 않은 스테이지입니다.");
        }
    }

    public void onCloseStageSelectPanel()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        stageSelectPanel.SetActive(false); // 스테이지 선택 패널 비활성화
    }
    public void onGoToTitle()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        SceneLoader.Load(titleSceneName);
    }

}
