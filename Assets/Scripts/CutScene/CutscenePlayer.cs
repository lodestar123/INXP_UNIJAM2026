using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Utils;

public class CutscenePlayer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image bgImage;
    [SerializeField] private List<Image> moveImagePool;
    [SerializeField] private List<TMP_Text> dialogueTexts;

    private CutsceneFrame[] _frames;
    private int _index;
    private bool _isPlaying;
    private System.Action _onComplete;
    private Sequence _seq;
    private bool _isTransitioning = false;

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
        if (_isTransitioning) return;
        _isTransitioning = true;

        _index++;
        if (_index >= _frames.Length) { EndCutscene(); return; }
        ShowFrame(_index);

        _isTransitioning = false;
    }
    private void ShowFrame(int index)
    {
        var frame = _frames[index];
        _seq?.Kill();

        // 배경
        bgImage.sprite = frame.bgSprite;

        // 이미지 전부 비활성화
        foreach (var img in moveImagePool)
            img.gameObject.SetActive(false);

        // 위치·스프라이트 세팅
        for (int i = 0; i < frame.moveImages?.Count; i++)
        {
            if (i >= moveImagePool.Count) break;
            var data = frame.moveImages[i];
            var img = moveImagePool[i];

            img.gameObject.SetActive(true);
            img.sprite = data.sprite;
            img.rectTransform.anchoredPosition = data.startPos; // 위치 먼저
            img.color = data.fadeSettings.useFade ? new Color(1f, 1f, 1f, 0f) : Color.white;
        }

        // 텍스트 세팅
        for (int i = 0; i < dialogueTexts.Count; i++)
        {
            if (i < frame.Texts.Count)
            {
                var textData = frame.Texts[i];
                dialogueTexts[i].text = textData.dialogueText;
                dialogueTexts[i].rectTransform.anchoredPosition = textData.textPos; // 위치 먼저
                dialogueTexts[i].alignment = textData.textAlignment;
                dialogueTexts[i].color = textData.fadeSettings.useFade ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
            else
            {
                dialogueTexts[i].text = string.Empty;
                dialogueTexts[i].color = Color.white;
            }
        }

        // Sequence로 이동·페이드 동시 실행
        _seq = DOTween.Sequence();
        _seq.OnKill(() => CustomLog.Info("Sequence가 Kill됨 - index: " + index));
        CustomLog.Info("Sequence 생성됨 - index: " + index);

        // 이미지 트윈
        for (int i = 0; i < frame.moveImages?.Count; i++)
        {
            if (i >= moveImagePool.Count) break;
            var data = frame.moveImages[i];
            var img = moveImagePool[i];

            if (data.duration > 0f)
                _seq.Join(img.rectTransform.DOAnchorPos(data.endPos, data.duration).SetEase(data.ease));

            if (data.fadeSettings.useFade)
            {
                _seq.Insert(data.fadeSettings.startDelay,
                    img.DOFade(1f, data.fadeSettings.fadeInDuration));

                if (data.fadeSettings.fadeOutDuration > 0f)
                    _seq.Insert(
                        data.fadeSettings.startDelay + data.fadeSettings.fadeInDuration,
                        img.DOFade(0f, data.fadeSettings.fadeOutDuration));
            }
        }

        // 텍스트 트윈
        for (int i = 0; i < frame.Texts?.Count; i++)
        {
            if (i >= dialogueTexts.Count) break;
            var textData = frame.Texts[i];

            if (textData.fadeSettings.useFade)
            {
                _seq.Insert(textData.fadeSettings.startDelay,
                    dialogueTexts[i].DOFade(1f, textData.fadeSettings.fadeInDuration));

                if (textData.fadeSettings.fadeOutDuration > 0f)
                    _seq.Insert(
                        textData.fadeSettings.startDelay + textData.fadeSettings.fadeInDuration,
                        dialogueTexts[i].DOFade(0f, textData.fadeSettings.fadeOutDuration));
            }
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