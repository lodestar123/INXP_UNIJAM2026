using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class HighScoreEntry
{
    public string key; // 사용자명
    public int value;  // 점수

    public HighScoreEntry(string key, int value)
    {
        this.key = key;
        this.value = value;
    }
}

[Serializable]
public class GameData
{
    public const int SkinCount = 10; // 전체 스킨 개수(필요시 조정)
    public float backGroundMusicVolume = 0.5f;
    public float effectSoundVolume = 0.5f;
    public int currentSkin = 0; // 현재 선택된 스킨 번호

    // Item Queue 저장용
    public ItemQueue itemQueue = new ItemQueue();

    // Board Fill Cursor 저장용
    public BoardFillCursor boardFillCursor = new BoardFillCursor();

    /// <summary>
    /// High Scores 저장용
    /// </summary>
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();

    /// <summary>
    /// Character Skins 획득 여부 저장용
    /// </summary>
    public List<bool> characterSkins;

    public GameData()
    {
        characterSkins = new List<bool>(new bool[SkinCount]);
    }
}

