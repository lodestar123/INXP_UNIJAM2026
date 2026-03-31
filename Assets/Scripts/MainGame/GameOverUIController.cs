using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameOverUIController : MonoBehaviour
{

    public TextMeshProUGUI gameResult; // 게임 결과 출력
    public TextMeshProUGUI alarm; // 기록 저장 여부 등 출력
    public TMP_InputField inputName; // 입력한 이름
    private bool isRecorded = false; // 저장 여부
    private bool isFirstClear = false; // 첫클리어 여부

    [Header("Scene Names")]
    [SerializeField] private string CutSceneName = "CutScene";
    [SerializeField] private string LobbySceneName = "LobbyScene";
    private void Start()
    {
        // GameSceneManager의 이벤트에 구독
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameOver += OnGameOverTextUpdate;
        }

        isRecorded = false; // 저장 초기화
        isFirstClear = false; // 첫클리어 초기화
    }

    private void OnDestroy()
    {

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameOver -= OnGameOverTextUpdate;

        }

    }


    /// <summary>
    /// 게임 오버 이후 뜨는 텍스트 업데이트 + 다음 스테이지 해금
    /// </summary>
    public void OnGameOverTextUpdate()
    {
        // 스테이지별 최고점수 기록 업데이트
        GameManager.Instance.UpdateStageHighScore(GameSceneManager.Instance.CurrentScore);

        int myScore = GameSceneManager.Instance.CurrentScore;

        int maxScore = GameManager.Instance.GameData.stageHighScore[GameManager.Instance.currentStageNum];

        //점수 출력
        gameResult.text = $"점수 : {myScore} 점";

        // 알람 메시지 출력
        if (myScore >= 10000 && !GameManager.Instance.GameData.stageUnlocked[GameManager.Instance.currentStageNum + 1])
        {
            alarm.text = "다음 스테이지가 해금되었습니다!";
            GameManager.Instance.UnlockNextStage(); //다음 스테이지 해금
            isFirstClear = true; // 첫클리어 체크
        }
        else if (myScore > maxScore)
        {
            alarm.text = "신기록!";
        }
        else
        {
            alarm.text = "";
        }
    }


    public void RecordScore()
    {
        if (isRecorded) return; // 이미 저장된 경우 무시
        try
        {
            GameManager.Instance.highScores.Add(inputName.text, GameSceneManager.Instance.CurrentScore);
            alarm.text = "기록이 저장되었습니다!";
        }
        catch
        {
            GameManager.Instance.highScores[inputName.text] = GameSceneManager.Instance.CurrentScore;
            alarm.text = "새로운 기록으로 저장되었습니다!";
        }

        isRecorded = true;
        // 수동 저장
        SaveLoadManager.Instance.SaveGame();

        // 서버 랭킹에 현재 스테이지 하이스코어 등록 (로그인된 유저 기준, 닉네임은 뒤끝 유저 닉네임으로 표시됨)
        BackendRank.Instance.RankInsertCurrentStageHighScore();
    }

    public void OnLobbyButton() // 로비 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생
        if (isFirstClear)
        {
            GameManager.Instance.nextSceneAfterCutscene = LobbySceneName; // 컷씬 이후 로비로 이동해야 함 지정
            SceneManager.LoadScene(CutSceneName);
        }
        else
        {
            SceneManager.LoadScene(LobbySceneName);
        }

    }
}
