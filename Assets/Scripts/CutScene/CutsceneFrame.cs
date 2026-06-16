using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
// using System.Drawing;

[System.Serializable]
public class MoveImageData
{
    public Sprite sprite;
    public Vector2 size; // 이미지 크기
    public FaidInOut fadeSettings;

    [Header("움직임 (필요없으면 duration = 0")]
    public Vector2 startPos;
    public Vector2 endPos;
    public float duration;
    public Ease ease;
}

[System.Serializable]
public class ItemImageData
{
    public Sprite sprite;
    public Vector2 size; // 이미지 크기
    public FaidInOut fadeSettings;
    public Vector2 spritePos; // 이미지 위치
}
[System.Serializable]
public class TextData
{
    [TextArea] public string dialogueText;
    public Vector2 textPos;
    public TextAlignmentOptions textAlignment = TextAlignmentOptions.Left; // 기본값 좌정렬
    public FaidInOut fadeSettings;

}
[System.Serializable]
public class FaidInOut
{
    [Header("페이드 (필요없으면 useFade = false)")]
    public bool useFade;
    public float startDelay; // 페이드 시작 지연
    public float fadeInDuration;
    public float fadeOutDuration;
}
[System.Serializable]
public class CutsceneFrame
{
    [Header("아이템 이미지")]
    public ItemImageData itemImage;

    [Header("텍스트")]
    public List<TextData> Texts;

    [Header("요소 이미지 리스트(없을 시 null)")]
    public List<MoveImageData> moveImages;

}
