using UnityEngine;
using Utils;

public class CutsceneManager : MonoBehaviour
{
    void Start()
    {
        PlayStoryCutscene(GameManager.Instance.currentStageNum);
        EndStoryCutscene();
    }

    public void PlayStoryCutscene(int stageIndex)
    {
        CustomLog.Info("컷씬 씬에서 컷씬을 재생합니다. 재생되는 컷: " + stageIndex);
        // 스토리 컷씬 재생 로직 구현
    }

    public void EndStoryCutscene()
    {
        SceneLoader.Load(GameManager.Instance.nextSceneAfterCutscene); // 컷씬이 끝나고 씬 이동
    }

}
