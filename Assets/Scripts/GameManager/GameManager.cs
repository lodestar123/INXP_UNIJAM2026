using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameData gamedata = new GameData();    // GameData (세이브 필요한 데이터)
    public GameData GameData => gamedata;

    public SoundManager soundManager { get; private set; }

    public Dictionary<string, int> highScores = new Dictionary<string, int>();
    public int currentStageNum = -1; // 현재 플레이 중인 스테이지 넘버 (스테이지 밖: -1)

    void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 컴포넌트 초기화
            soundManager = GetComponent<SoundManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 하이스코어 업데이트
    public void UpdateHighScore(string levelName, int score)
    {
        if (!highScores.ContainsKey(levelName) || highScores[levelName] < score)
        {
            highScores[levelName] = score;
            SaveLoadManager.Instance?.SaveGame(); // 즉시 저장
        }
    }

    /// <summary>
    /// 스테이지별 하이스코어 업데이트
    /// </summary>
    public void UpdateStageHighScore(int highScore)
    {
        if (gamedata.stageHighScore[currentStageNum] >= highScore) return;

        gamedata.stageHighScore[currentStageNum] = highScore;
        SaveLoadManager.Instance?.SaveGame();
    }

    /// <summary>
    /// 캐릭터 스킨 획득 여부 업데이트
    /// </summary>
    public void UpdateCharacterSkin(int skinNum, bool isUnlocked)
    {
        // 유효 범위 체크
        if (skinNum < 0 || skinNum >= GameData.SkinCount) return;

        // 리스트 길이 보정 (대비용)
        while (gamedata.characterSkins.Count < GameData.SkinCount)
        {
            gamedata.characterSkins.Add(false);
        }

        gamedata.characterSkins[skinNum] = isUnlocked;
        SaveLoadManager.Instance?.SaveGame();
    }

    /// <summary>
    /// 현재 착용중인 캐릭터 스킨 변경
    /// </summary>
    public void ChangeCurrentSkin(int skinNum)
    {
        // 유효 범위 및 획득 여부 체크
        if (skinNum < 0 || skinNum >= GameData.SkinCount)
            return;
        // 리스트 길이 보정 (대비용)
        while (gamedata.characterSkins.Count < GameData.SkinCount)
        {
            gamedata.characterSkins.Add(false);
        }

        if (!gamedata.characterSkins[skinNum]) return; // 미획득 스킨은 선택 불가

        gamedata.currentSkin = skinNum;
        SaveLoadManager.Instance?.SaveGame();
    }

    /// <summary>
    /// (내부용) 스테이지 해금
    /// </summary>
    private void UnlockStage(int stageNum)
    {
        // 유효 범위 체크
        if (stageNum < 0 || stageNum >= GameData.StageCount) return;

        // 리스트 길이 보정 (대비용)
        while (gamedata.stageUnlocked.Count < GameData.StageCount)
        {
            gamedata.stageUnlocked.Add(false);
        }

        gamedata.stageUnlocked[stageNum] = true;
        SaveLoadManager.Instance?.SaveGame();
    }

    /// <summary>
    /// 다음 스테이지 해금
    /// </summary>
    public void UnlockNextStage()
    {
        int nextStage = currentStageNum + 1;
        if (nextStage >= GameData.StageCount) return; // 마지막 스테이지면 해금 불필요
        if (gamedata.stageUnlocked[nextStage]) return; // 이미 해금됨

        UnlockStage(nextStage);
    }
}




