using UnityEngine;
using Utils;

public class CutsceneManager : MonoBehaviour
{

    public static CutsceneManager Instance { get; private set; }

    [SerializeField] private CutsceneData[] cutsceneDatas; // 스테이지별 컷씬 데이터 배열

    [SerializeField] private CutscenePlayer player;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        PlayCutscene(GameManager.Instance.currentStageNum);
    }

    public void PlayCutscene(int stageIndex)
    {
        CustomLog.Info("재생되는 컷: " + cutsceneDatas[stageIndex + 1].name);

        var data = cutsceneDatas[stageIndex + 1]; // stageIndex는 0부터 시작하므로 +1 (0번 컷씬은 인트로)
        player.Play(data.frames, EndCutscene); // 끝나면 콜백으로 End 호출
    }


    public void EndCutscene()
    {
        SceneLoader.Load(GameManager.Instance.nextSceneAfterCutscene); // 컷씬이 끝나고 씬 이동
    }

}
