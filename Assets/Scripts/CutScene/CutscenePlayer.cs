using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Utils;
using Core.Input;

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

    public void Play(CutsceneFrame[] frames, System.Action onComplete)
    {
        _frames = frames;
        _index = 0;
        _onComplete = onComplete;
        //_isPlaying = true;
        _isPlaying = false;
        gameObject.SetActive(true);
        ShowFrame(0);
    }

    private void Update()
    {
        if (!_isPlaying) return;

        if (UnifiedInputManager.Instance.WasTappedThisFrame)
            NextFrame();
        /*if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            NextFrame();
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            NextFrame();*/
    }

    private void NextFrame()
    {
        if (!_isPlaying) return;
        _isPlaying = false; // 입력 차단

        _seq?.Kill();
        _seq = null;

        _index++;
        if (_index >= _frames.Length) { EndCutscene(); return; }
        ShowFrame(_index);

        //_isPlaying = true; // 다음 프레임이 보여지면 입력 허용 (트윈이 있으면 트윈 완료 후 허용하도록 변경)
    }
    private void ShowFrame(int index)
    {
        var frame = _frames[index];
        _seq?.Kill();

        // 배경
        // bgImage.sprite = frame.bgSprite;

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
            img.rectTransform.anchoredPosition = data.startPos; // 위치
            img.rectTransform.sizeDelta = data.size; // 크기
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
                dialogueTexts[i].color = textData.fadeSettings.useFade ? new Color(52 / 255f, 63 / 255f, 63 / 255f, 0f) : Color.white; // 페이드 여부에 따라 초기 투명도 설정
            }
            else
            {
                dialogueTexts[i].text = string.Empty;
                dialogueTexts[i].color = Color.white;
            }
        }
        bool hasTween = false;
        _seq = DOTween.Sequence();
        // 이미지 트윈
        for (int i = 0; i < frame.moveImages?.Count; i++)
        {
            if (i >= moveImagePool.Count) break;
            var data = frame.moveImages[i];
            var img = moveImagePool[i];

            if (data.duration > 0f)
            {
                _seq.Join(img.rectTransform.DOAnchorPos(data.endPos, data.duration).SetEase(data.ease));
                hasTween = true;
                CustomLog.Info($"이동 트윈 추가 - index:{index} i:{i} duration:{data.duration}");
            }

            if (data.fadeSettings.useFade)
            {
                if (data.fadeSettings.fadeInDuration > 0f)
                {
                    img.color = new Color(1f, 1f, 1f, 0f);
                    _seq.Insert(data.fadeSettings.startDelay,
                        img.DOFade(1f, data.fadeSettings.fadeInDuration));
                    hasTween = true;
                    CustomLog.Info($"이미지 페이드인 추가 - index:{index} i:{i} fadeIn:{data.fadeSettings.fadeInDuration}");
                }
                else
                {
                    img.color = Color.white;
                }

                if (data.fadeSettings.fadeOutDuration > 0f)
                {
                    float outStart = data.fadeSettings.startDelay + data.fadeSettings.fadeInDuration;
                    _seq.Insert(outStart, img.DOFade(0f, data.fadeSettings.fadeOutDuration));
                    hasTween = true;
                    CustomLog.Info($"이미지 페이드아웃 추가 - outStart:{outStart} fadeOutDuration:{data.fadeSettings.fadeOutDuration} SeqDuration:{_seq.Duration()}");
                }
            }
        }

        // 텍스트 트윈
        for (int i = 0; i < frame.Texts?.Count; i++)
        {
            if (i >= dialogueTexts.Count) break;
            var textData = frame.Texts[i];

            if (textData.fadeSettings.useFade)
            {
                if (textData.fadeSettings.fadeInDuration > 0f)
                {
                    dialogueTexts[i].color = new Color(52 / 255f, 63 / 255f, 63 / 255f, 0f);
                    _seq.Insert(textData.fadeSettings.startDelay,
                        dialogueTexts[i].DOFade(1f, textData.fadeSettings.fadeInDuration));
                    hasTween = true;
                    CustomLog.Info($"텍스트 페이드인 추가 - index:{index} i:{i}");
                }
                else
                {
                    dialogueTexts[i].color = Color.white;
                }

                if (textData.fadeSettings.fadeOutDuration > 0f)
                {
                    float outStart = textData.fadeSettings.startDelay + textData.fadeSettings.fadeInDuration;
                    _seq.Insert(outStart, dialogueTexts[i].DOFade(0f, textData.fadeSettings.fadeOutDuration));
                    hasTween = true;
                    CustomLog.Info($"텍스트 페이드아웃 추가 - outStart:{outStart} fadeOutDuration:{textData.fadeSettings.fadeOutDuration} SeqDuration:{_seq.Duration()}");
                }
            }
        }

        _seq.OnKill(() => CustomLog.Info($"Kill - index:{index} hasTween:{hasTween}"));

        if (!hasTween)
        {
            CustomLog.Info($"빈 Sequence - index:{index}, Kill 처리");
            _seq.Kill();
            _seq = null;
            _isPlaying = true; // 트윈 없으면 즉시 입력 허용
        }
        else
        {
            _seq.OnComplete(() => _isPlaying = true); // 트윈 완료 후 입력 허용
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