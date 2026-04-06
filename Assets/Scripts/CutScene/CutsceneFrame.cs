using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

[System.Serializable]
public class MoveImageData
{
    public Sprite sprite;
    public Vector2 startPos;
    public Vector2 endPos;
    public float duration;
    public Ease ease;
}

[System.Serializable]
public class CutsceneFrame
{
    [Header("배경 이미지")]
    public Sprite bgSprite;

    [Header("텍스트")]
    [TextArea] public string dialogueText;
    public Vector2 textPos;

    [Header("움직이는 이미지 리스트(없을 시 null)")]
    public List<MoveImageData> moveImages;

}
