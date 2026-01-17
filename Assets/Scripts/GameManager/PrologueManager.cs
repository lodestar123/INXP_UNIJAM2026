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

    [Header("Tutorial Settings")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private float tutorialFadeDuration = 0.5f;

    private int currentPrologueIndex = 0;
    private bool isPrologueActive = false;
    private bool isTutorialActive = false;
    private Sequence _buttonBlinkSequence; // 버튼 깜빡임 애니메이션 시퀀스
    private bool isProcessingClick = false; 

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
        // 튜토리얼 중일 때 - 한 번 터치로 종료
        if (isTutorialActive)
        {
            if (UnifiedInputManager.Instance != null && UnifiedInputManager.Instance.WasTappedThisFrame && !isProcessingClick)
            {
                SkipTutorial();
            }
            return;
        }

        // 프롤로그 중일 때
        if (!isPrologueActive) return;

        if (UnifiedInputManager.Instance != null && UnifiedInputManager.Instance.WasTappedThisFrame)
        {
            // 클릭 처리 중이면 무시 (광속 클릭 방지)
            if (isProcessingClick) return;

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
                // 인덱스 6 이상일 때 배경을 밝은 색, 텍스트를 검정색으로 
                if (backgroundImage != null)
                {
                    backgroundImage.color = new Color(1.0f, 0.8f, 0.54f, 1.0f);
                }

                prologueText.color = new Color(0f, 0f, 0f, 1f); // 알파값을 1로 설정
                if (prologueSkipText != null)
                {
                    prologueSkipText.color = new Color(0f, 0f, 0f, 1f); // 알파값을 1로 설정
                }
                
                // nextButton 검정색으로 설정
                if (nextButton != null && nextButton.image != null)
                {
                    nextButton.image.color = Color.black;
                }
            }
            else
            {
                if (backgroundImage != null)
                {
                    backgroundImage.color = Color.black;
                }

                prologueText.color = new Color(1f, 1f, 1f, 1f); // 알파값을 1로 설정
                if (prologueSkipText != null)
                {
                    prologueSkipText.color = new Color(1f, 1f, 1f, 1f); // 알파값을 1로 설정
                }
                
                if (nextButton != null && nextButton.image != null)
                {
                    nextButton.image.color = Color.white;
                }
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
            }
        }
    }

    public void OnNextPrologue()
    {
        if (!isPrologueActive) return;
        if (isProcessingClick) return;

        isProcessingClick = true; 
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
                    isProcessingClick = false;
                });
            }
            else
            {
                DisplayPrologue(currentPrologueIndex);
               
                isProcessingClick = false;
            }
        }
    }

    /// <summary>
    /// 프롤로그 스킵
    /// </summary>
    public void SkipPrologue()
    {
        if (!isPrologueActive) return;
        if (isProcessingClick) return; 

        isProcessingClick = true; 
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
        isProcessingClick = false;

        // 버튼 깜빡임 완전히 중지
        StopButtonBlink();

        // 텍스트와 패널만 페이드아웃

        Sequence fadeOutSequence = DOTween.Sequence();

        if (prologueText != null)
        {
            fadeOutSequence.Join(prologueText.DOFade(0f, endFadeDuration));
        }

        if (prologueSkipText != null)
        {
            fadeOutSequence.Join(prologueSkipText.DOFade(0f, endFadeDuration));
        }

        if (nextButton != null && nextButton.image != null)
        {
            Image nextButtonImage = nextButton.image;
            nextButtonImage.DOKill();
            fadeOutSequence.Join(nextButtonImage.DOFade(0f, endFadeDuration));
        }

        if (skipButton != null && skipButton.image != null)
        {
            Image skipButtonImage = skipButton.image;
            skipButtonImage.DOKill();
            fadeOutSequence.Join(skipButtonImage.DOFade(0f, endFadeDuration));
        }

        if (GameManager.Instance != null && GameManager.Instance.soundManager != null && GameManager.Instance.soundManager.bgmPlayer != null)
        {
            AudioSource bgmPlayer = GameManager.Instance.soundManager.bgmPlayer;
            fadeOutSequence.Join(bgmPlayer.DOFade(0f, endFadeDuration));
        }

        // 페이드아웃 완료 후 패널 비활성화 및 콜백 호출
        fadeOutSequence.OnComplete(() =>
        {
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

            // 프롤로그 완료 후 튜토리얼 표시
            ShowTutorial();
            isProcessingClick = false; // 프롤로그 완료 시 클릭 처리 해제
        });
    }

    /// <summary>
    /// 튜토리얼 이미지 표시
    /// </summary>
    private void ShowTutorial()
    {
        // 튜토리얼 패널이 없으면 바로 본 게임으로 전환
        if (tutorialPanel == null || tutorialImage == null)
        {
            OnTutorialCompleted();
            return;
        }

        isTutorialActive = true;
        isProcessingClick = false;

        // 튜토리얼 패널 활성화 및 페이드인
        tutorialPanel.SetActive(true);
        
        CanvasGroup tutorialCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
        if (tutorialCanvasGroup == null)
        {
            tutorialCanvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
        }

        tutorialCanvasGroup.alpha = 0f;
        tutorialCanvasGroup.DOFade(1f, tutorialFadeDuration);
    }

    /// <summary>
    /// 튜토리얼 스킵 (한 번 터치로 종료)
    /// </summary>
    private void SkipTutorial()
    {
        if (!isTutorialActive || isProcessingClick) return;

        isProcessingClick = true;
        isTutorialActive = false;

        if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
        {
            GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        }

        OnTutorialCompleted();
        isProcessingClick = false;
    }

    /// <summary>
    /// 튜토리얼 완료 후 본 게임으로 전환
    /// </summary>
    private void OnTutorialCompleted()
    {
        // 프롤로그 완료 콜백 호출 (본 게임 시작)
        onPrologueCompleted?.Invoke();
    }

    // 프롤로그 완료 시 호출되는 콜백
    private System.Action onPrologueCompleted;

    // 프롤로그 완료 콜백 설정
    public void SetOnCompletedCallback(System.Action callback)
    {
        onPrologueCompleted = callback;
    }

    // 프롤로그 완료 시 호출 (더 이상 사용하지 않음, OnTutorialCompleted에서 호출)
    private void OnPrologueCompleted()
    {
        // 이제는 OnTutorialCompleted에서 호출됨
    }


}
