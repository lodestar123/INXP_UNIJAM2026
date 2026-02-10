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
    public List<bool> characterSkins = new List<bool>();
    public int currentSkinNum = 0;

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
    /// 캐릭터 스킨 획득 여부 업데이트
    /// </summary>
    public void UpdateCharacterSkin(int skinNum, bool isUnlocked)
    {
        // 유효 범위 체크
        if (skinNum < 0 || skinNum >= GameData.SkinCount) return;

        // 리스트 길이 보정 (혹시라도)
        while (characterSkins.Count < GameData.SkinCount)
        {
            characterSkins.Add(false);
        }

        characterSkins[skinNum] = isUnlocked;
        // GameData에도 반영
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
        // 리스트 길이 보정 (혹시라도)
        while (characterSkins.Count < GameData.SkinCount)
        {
            characterSkins.Add(false);
        }

        if (!characterSkins[skinNum]) return; // 미획득 스킨은 선택 불가

        currentSkinNum = skinNum;
        gamedata.currentSkin = skinNum;
        SaveLoadManager.Instance?.SaveGame();
    }
}




