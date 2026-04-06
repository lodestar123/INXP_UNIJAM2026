using UnityEngine;
using Utils;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CutsceneManager : MonoBehaviour
{

    public static CutsceneManager Instance { get; private set; }

    [SerializeField] private CutsceneData[] itemCutsceneDatas;   // 아이템 컷씬 데이터 배열 (인덱스: 스테이지 번호)
    [SerializeField] private CutsceneData[] cutsceneDatas; // 스테이지별 컷씬 데이터 배열(인덱스: 스테이지 번호+1)

    [SerializeField] private CutscenePlayer player;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        int stageIndex = GameManager.Instance.currentStageNum;

        // 인트로(0번)는 아이템 컷씬 없으므로 스킵
        if (stageIndex == -1 || itemCutsceneDatas[stageIndex] == null)
        {
            PlayStageCutscene(stageIndex);
        }
        else
        {
            PlayItemCutscene(stageIndex);
        }
    }

    private void PlayItemCutscene(int stageIndex)
    {
        CustomLog.Info("아이템 컷씬을 재생합니다. 스테이지: " + stageIndex);
        player.Play(itemCutsceneDatas[stageIndex].frames, () => PlayStageCutscene(stageIndex));
    }

    public void PlayStageCutscene(int stageIndex)
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
[CustomEditor(typeof(CutsceneManager))]
public class CutsceneManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("씬 리로드"))
        {
            EditorSceneManager.LoadScene(EditorSceneManager.GetActiveScene().name);
        }
    }
}
