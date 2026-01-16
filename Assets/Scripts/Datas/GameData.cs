using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public float backGroundMusicVolume = 0.5f;
    public float effectSoundVolume = 0.5f;
    public int highScore = 0;

    // Item Queue 저장용
    public ItemQueue itemQueue = new ItemQueue();

    // Board Fill Cursor 저장용
    public BoardFillCursor boardFillCursor = new BoardFillCursor();
}

