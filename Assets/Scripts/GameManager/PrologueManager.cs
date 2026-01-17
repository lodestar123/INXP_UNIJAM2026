using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Core.Input;

/// <summary>
/// 프롤로그를 관리하는 클래스
/// </summary>
public class PrologueManager : MonoBehaviour
{
    [Header("Prologue Settings")]
    [SerializeField] private GameObject prologuePanel;
    [SerializeField] private List<string> prologueTexts = new List<string>();
    [SerializeField] private TextMeshProUGUI prologueText;
    [SerializeField] private TextMeshProUGUI prologueSkipText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float endFadeDuration = 0.8f;

    private int currentPrologueIndex = 0;
    private bool isPrologueActive = false;
    private Sequence _buttonBlinkSequence; // 버튼 깜빡임 애니메이션 시퀀스

    /// 프롤로그 시작 (항상 표시)
    public bool ShowPrologueIfNeeded()
    {
        if (prologuePanel == null || prologueTexts.Count == 0)
        {
            return false;
        }

        ShowPrologue();
        return true;
    }

    private void ShowPrologue()
    {
        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Prologue);

        isPrologueActive = true;
        currentPrologueIndex = 0;

        // 스킵 버튼 활성화
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
        }

        // 패널 활성화 및 페이드인 효과
        if (prologuePanel != null)
        {
            prologuePanel.SetActive(true);

            CanvasGroup canvasGroup = prologuePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = prologuePanel.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration);
        }

        DisplayPrologue(currentPrologueIndex);

    }

    private void Update()
    {
        if (!isPrologueActive) return;

        if (UnifiedInputManager.Instance != null && UnifiedInputManager.Instance.WasTappedThisFrame)
        {
            // 스킵 버튼 터치 확인
            if (skipButton != null && skipButton.gameObject.activeSelf && IsPointerOverButton(skipButton))
            {
                SkipPrologue();
                return;
            }

            // Next 버튼 또는 화면 터치
            OnNextPrologue();
        }
    }

    /// <summary>
    /// 포인터가 버튼 위에 있는지 확인
    /// </summary>
    private bool IsPointerOverButton(Button button)
    {
        if (button == null || button.gameObject == null) return false;

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null) return false;

        Vector2 pointerPos = UnifiedInputManager.Instance.PointerPosition;

        // 월드 좌표를 캔버스 좌표로 변환
        Canvas canvas = button.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                pointerPos,
                canvas.worldCamera,
                out Vector2 localPoint);

            return rectTransform.rect.Contains(localPoint);
        }

        return false;
    }

    private void DisplayPrologue(int index)
    {
        if (prologueText != null && index < prologueTexts.Count)
        {
            prologueText.text = prologueTexts[index];

            if (index == 9)
            {
                prologueText.fontSize = 200;
            }
            else
            {
                prologueText.fontSize = 70;
            }

            if (index >= 6)
            {
                // 인덱스 6 이상일 때 배경을 연회색, 텍스트를 검정색으로 
                if (backgroundImage != null)
                {
                    new Color(1.0f, 0.8f, 0.54f, 1.0f);

                }

                prologueText.color = new Color(0f, 0f, 0f, 0);
                //prologueSkipText.color = new Color(0f, 0f, 0f, 0);
            }
            else
            {
                if (backgroundImage != null)
                {
                    backgroundImage.color = Color.black;
                }

                prologueText.color = new Color(1f, 1f, 1f, 0);
                //prologueSkipText.color = new Color(1f, 1f, 1f, 0);
            }

            // 텍스트 페이드인 완료 후 버튼 깜빡임 시작
            prologueText.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                StartButtonBlink();
            });
        }
    }

    private void StartButtonBlink()
    {
        StopButtonBlink();

        if (nextButton == null) return;

        nextButton.gameObject.SetActive(true);

        Image buttonImage = nextButton.GetComponent<Image>();
        if (buttonImage == null) return;

        // 깜빡임 애니메이션 (alpha를 0.3 ~ 1.0 사이로 반복)
        _buttonBlinkSequence = DOTween.Sequence();
        _buttonBlinkSequence.Append(buttonImage.DOFade(0.3f, 0.5f).SetEase(Ease.InOutSine))
                           .Append(buttonImage.DOFade(1f, 0.5f).SetEase(Ease.InOutSine))
                           .SetLoops(-1); // 무한 반복
    }

    private void StopButtonBlink()
    {
        if (_buttonBlinkSequence != null)
        {
            _buttonBlinkSequence.Kill();
            _buttonBlinkSequence = null;
        }

        if (nextButton != null)
        {
            Image buttonImage = nextButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.DOKill();
                buttonImage.DOFade(1f, 0f);
            }

            nextButton.gameObject.SetActive(false);
        }
    }

    public void OnNextPrologue()
    {
        if (!isPrologueActive) return;

        StopButtonBlink();

        if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
        {
            GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        }

        currentPrologueIndex++;

        if (currentPrologueIndex >= prologueTexts.Count)
        {
            EndPrologue();
        }
        else
        {
            if (prologueText != null)
            {
                prologueText.DOFade(0f, fadeDuration).OnComplete(() =>
                {
                    DisplayPrologue(currentPrologueIndex);
                });
            }
            else
            {
                DisplayPrologue(currentPrologueIndex);
            }
        }
    }

    /// <summary>
    /// 프롤로그 스킵
    /// </summary>
    public void SkipPrologue()
    {
        if (!isPrologueActive) return;

        StopButtonBlink();

        if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
        {
            GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        }

        // 스킵 버튼 비활성화
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

        EndPrologue();
    }

    private void EndPrologue()
    {
        isPrologueActive = false;

        // 버튼 깜빡임 중지
        StopButtonBlink();

        // 스킵 버튼 비활성화
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

        // PlayerPrefs 저장 제거 (항상 표시하도록 변경)
        // 배경은 페이드아웃하지 않고, 텍스트와 패널만 페이드아웃

        Sequence fadeOutSequence = DOTween.Sequence();

        // 텍스트 페이드아웃
        if (prologueText != null)
        {
            fadeOutSequence.Join(prologueText.DOFade(0f, endFadeDuration));
        }

        // 스킵 텍스트 페이드아웃
        if (prologueSkipText != null)
        {
            fadeOutSequence.Join(prologueSkipText.DOFade(0f, endFadeDuration));
        }

        // BGM 페이드아웃
        if (GameManager.Instance != null && GameManager.Instance.soundManager != null && GameManager.Instance.soundManager.bgmPlayer != null)
        {
            AudioSource bgmPlayer = GameManager.Instance.soundManager.bgmPlayer;
            fadeOutSequence.Join(bgmPlayer.DOFade(0f, endFadeDuration));
        }

        // 페이드아웃 완료 후 패널 비활성화 및 콜백 호출
        fadeOutSequence.OnComplete(() =>
        {
            if (prologuePanel != null)
            {
                //prologuePanel.SetActive(false);
            }

            // BGM 정지 및 volume 복원
            if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
            {
                GameManager.Instance.soundManager.StopBGM();
                // BGM volume 복원 (다음 BGM 재생을 위해)
                if (GameManager.Instance.soundManager.bgmPlayer != null && GameManager.Instance.GameData != null)
                {
                    GameManager.Instance.soundManager.bgmPlayer.volume = GameManager.Instance.GameData.backGroundMusicVolume;
                }
            }

            OnPrologueCompleted();
        });
    }

    // 프롤로그 완료 시 호출되는 콜백
    private System.Action onPrologueCompleted;

    // 프롤로그 완료 콜백 설정
    public void SetOnCompletedCallback(System.Action callback)
    {
        onPrologueCompleted = callback;
    }

    // 프롤로그 완료 시 호출
    private void OnPrologueCompleted()
    {
        onPrologueCompleted?.Invoke();
    }


}
