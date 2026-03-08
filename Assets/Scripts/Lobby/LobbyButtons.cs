using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyButtons : MonoBehaviour
{

    public void LoadStage(int stageIndex)
    {
        GameManager.Instance.currentStageNum = stageIndex;
        SceneManager.LoadScene(stageIndex + 2); // 씬 빌드 순서에 따라 0: 타이틀, 1: 로비, 2: Stage1, 3: Stage2, ...
    }
}
