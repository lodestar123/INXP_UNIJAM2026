using UnityEngine;
using TMPro;
using Utils;
public class GameOverUIController : MonoBehaviour
{

    public TextMeshProUGUI gameResult; // 게임 결과 출력
    public TextMeshProUGUI alarm; // 기록 저장 여부 등 출력
    public TMP_InputField inputName; // 입력한 이름
    private bool isRecorded = false; // 저장 여부

    public int clearScore = 10000; // 클리어 점수 기준

    [Header("Scene Names")]
    [SerializeField] private string CutSceneName = "CutScene";
    [SerializeField] private string LobbySceneName = "LobbyScene";

    [Header("Buttons")]
    [SerializeField] private GameObject GiveButton; // 선물하러 가기 버튼
    [SerializeField] private GameObject LobbyButton; // 로비로 버튼
    [SerializeField] private GameObject RestartButton; // 다시하기 버튼

    private void Start()
    {
        CustomLog.Info("GameOverUIController 초기화");

        // GameSceneManager의 이벤트에 구독
        // if (GameSceneManager.Instance != null)
        //{
        //    GameSceneManager.Instance.OnGameOver += OnGameOverTextUpdate;
        //}

        GiveButton.SetActive(false); // 선물하러 가기 버튼 비활성화
        RestartButton.SetActive(true); // 다시하기 버튼 활성화
        LobbyButton.SetActive(true); // 로비 버튼 활성화

        isRecorded = false; // 저장 초기화
    }
    private void OnEnable()
    {
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameOver += OnGameOverTextUpdate;
        }
    }

    private void OnDisable()
    {
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameOver -= OnGameOverTextUpdate;
        }
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
        gameResult.text = myScore.ToString();
        CustomLog.Info("점수 출력");

        // 알람 메시지 출력
        if (myScore >= clearScore && !GameManager.Instance.GameData.stageUnlocked[GameManager.Instance.currentStageNum + 1])
        {
            alarm.text = "다음 스테이지가 해금되었습니다!";
            GameManager.Instance.UnlockNextStage(); //다음 스테이지 해금

            CustomLog.Info($"스테이지 {GameManager.Instance.currentStageNum + 1}이 해금되었습니다.");

            GiveButton.SetActive(true); // 선물하러 가기 버튼 활성화
            RestartButton.SetActive(false); // 다시하기 버튼 비활성화
            LobbyButton.SetActive(false); // 로비 버튼 비활성화
        }
        else if (myScore < clearScore)
        {
            CustomLog.Info("실패");
            alarm.text = "실패";
        }
        else if (myScore >= maxScore)
        {
            CustomLog.Info("신기록!");
            alarm.text = "신기록!";
        }
        else
        {
            CustomLog.Info("성공");
            alarm.text = "성공!";
        }

        //게임 종료 직후 스테이지 하이스코어를 뒤끝 랭킹에 반영
        BackendRank.Instance.RankInsertCurrentStageHighScore();
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

    }

    public void OnGiveButton() // 선물하러 가기 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        GameManager.Instance.nextSceneAfterCutscene = LobbySceneName; // 컷씬 이후 로비로 이동해야 함 지정
        SceneLoader.Load(CutSceneName);
    }
    public void OnLobbyButton() // 로비 버튼 클릭
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick); // 버튼 클릭 효과음 재생

        SceneLoader.Load(LobbySceneName);

    }
}
