using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class HighScoreEntry
{
    public string key;
    public int value;

    public HighScoreEntry(string key, int value)
    {
        this.key = key;
        this.value = value;
    }
}
[Serializable]
public class GameData
{
    public float backGroundMusicVolume = 0.5f;
    public float effectSoundVolume = 0.5f;

    // Item Queue 저장용
    public ItemQueue itemQueue = new ItemQueue();

    // Board Fill Cursor 저장용
    public BoardFillCursor boardFillCursor = new BoardFillCursor();


    // High Scores 저장용
    public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
}

