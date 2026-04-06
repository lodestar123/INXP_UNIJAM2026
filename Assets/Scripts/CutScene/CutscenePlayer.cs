using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CutscenePlayer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image bgImage;
    [SerializeField] private List<Image> moveImagePool; // 미리 만들어둔 Image 오브젝트들
    [SerializeField] private TMP_Text dialogueText;

    private CutsceneFrame[] _frames;
    private int _index;
    private bool _isPlaying;
    private System.Action _onComplete;
    private Sequence _seq;

    public void Play(CutsceneFrame[] frames, System.Action onComplete)
    {
        _frames = frames;
        _index = 0;
        _onComplete = onComplete;
        _isPlaying = true;
        gameObject.SetActive(true);
        ShowFrame(0);
    }

    private void Update()
    {
        if (!_isPlaying) return;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            NextFrame();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            NextFrame();
    }

    private void NextFrame()
    {
        _index++;
        if (_index >= _frames.Length) { EndCutscene(); return; }
        ShowFrame(_index);
    }

    private void ShowFrame(int index)
    {
        var frame = _frames[index];

        // DOTween 정리
        _seq?.Kill();

        // 배경
        bgImage.sprite = frame.bgSprite;

        // 텍스트
        dialogueText.text = frame.dialogueText;
        dialogueText.rectTransform.anchoredPosition = frame.textPos;

        // 움직이는 이미지들 전부 비활성화 후 필요한 것만 켜기
        foreach (var img in moveImagePool)
            img.gameObject.SetActive(false);

        if (frame.moveImages == null || frame.moveImages.Count == 0) return;

        _seq = DOTween.Sequence();

        for (int i = 0; i < frame.moveImages.Count; i++)
        {
            if (i >= moveImagePool.Count) break; // 풀 범위 초과 방어

            var data = frame.moveImages[i];
            var img = moveImagePool[i];

            img.gameObject.SetActive(true);
            img.sprite = data.sprite;
            img.rectTransform.anchoredPosition = data.startPos;

            // 각 이미지 트윈을 Sequence에 동시 삽입 (Join = 동시 재생)
            _seq.Join(
                img.rectTransform
                   .DOAnchorPos(data.endPos, data.duration)
                   .SetEase(data.ease)
            );
        }
    }

    private void EndCutscene()
    {
        _isPlaying = false;
        _seq?.Kill();
        foreach (var img in moveImagePool)
            img.gameObject.SetActive(false);
        gameObject.SetActive(false);
        _onComplete?.Invoke();
    }
}