using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private LobbyPanel lobbyPanel; // 로비 패널 참조

    [Header("Scene Names")]
    [SerializeField] private string GameSceneName = "MainScene";

    public void Start()
    {
        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Title);
    }
    public void onStageButton(int stageIndex)
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);

        if (GameManager.Instance.GameData.stageUnlocked[stageIndex])
        {
            lobbyPanel.openStageSelectPanel(stageIndex);
        }
        else
        {
            Debug.Log("해금되지 않은 스테이지입니다.");
        }

    }
}
