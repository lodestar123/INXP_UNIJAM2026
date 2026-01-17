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
    [SerializeField] private TextMeshProUGUI prologueTextDisplay;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button nextButton; 
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float endFadeDuration = 3f; 

    private int currentPrologueIndex = 0;
    private bool isPrologueActive = false;
    private const string PROLOGUE_SHOWN_KEY = "PrologueShown";
    private Sequence _buttonBlinkSequence; // 버튼 깜빡임 애니메이션 시퀀스

    public bool IsPrologueShown()
    {
        return PlayerPrefs.GetInt(PROLOGUE_SHOWN_KEY, 0) == 1;
    }

    /// 프롤로그 시작 (최초 1회만 실행)
    public bool ShowPrologueIfNeeded()
    {
        if (IsPrologueShown())
        {
            return false;
        }

        if (prologuePanel == null || prologueTexts.Count == 0)
        {
            return false;
        }

        ShowPrologue();
        return true;
    }

    private void ShowPrologue()
    {
        isPrologueActive = true;
        currentPrologueIndex = 0;
        
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
            OnNextPrologue();
        }
    }

    private void DisplayPrologue(int index)
    {
        if (prologueTextDisplay != null && index < prologueTexts.Count)
        {
            prologueTextDisplay.text = prologueTexts[index];
            
            if (index == 8)
            {
                prologueTextDisplay.fontSize = 200;
            }
            else
            {
                 prologueTextDisplay.fontSize = 70;
            }
            
            if (index >= 5)
            {
                // 인덱스 5 이상일 때 배경을 하얀색, 텍스트를 검정색으로 
                if (backgroundImage != null)
                {
                    backgroundImage.color = Color.gray;
                }
                
                prologueTextDisplay.color = new Color(0f, 0f, 0f, 0);
            }
            else
            {
                if (backgroundImage != null)
                {
                    backgroundImage.color = Color.black;
                }
                
                prologueTextDisplay.color = new Color(1f, 1f, 1f, 0); 
            }
            
            // 텍스트 페이드인 완료 후 버튼 깜빡임 시작
            prologueTextDisplay.DOFade(1f, fadeDuration).OnComplete(() =>
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
            if (prologueTextDisplay != null)
            {
                prologueTextDisplay.DOFade(0f, fadeDuration).OnComplete(() =>
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

    private void EndPrologue()
    {
        isPrologueActive = false;

        // 버튼 깜빡임 중지
        StopButtonBlink();

        // PlayerPrefs에 프롤로그 표시 여부 저장
        PlayerPrefs.SetInt(PROLOGUE_SHOWN_KEY, 1);
        PlayerPrefs.Save();

        // 페이드 아웃 후 패널 비활성화 (마지막 페이드아웃은 천천히)
        if (prologueTextDisplay != null)
        {
            prologueTextDisplay.DOFade(0f, endFadeDuration).OnComplete(() =>
            {
                if (prologuePanel != null)
                {
                    //prologuePanel.SetActive(false);
                }
                OnPrologueCompleted();
            });
        }
        else
        {
            if (prologuePanel != null)
            {
                prologuePanel.SetActive(false);
            }
            OnPrologueCompleted();
        }
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

#if UNITY_EDITOR
    // 프롤로그 재시청 상태로 초기화 (테스트용)
    [ContextMenu("Reset Prologue (테스트용)")]
    public void ResetPrologue()
    {
        int beforeValue = PlayerPrefs.GetInt(PROLOGUE_SHOWN_KEY, 0);
        PlayerPrefs.DeleteKey(PROLOGUE_SHOWN_KEY);
        PlayerPrefs.Save();
        int afterValue = PlayerPrefs.GetInt(PROLOGUE_SHOWN_KEY, 0);
        Debug.Log($"[PrologueManager] ResetPrologue 실행됨 - 이전 값: {beforeValue}, 삭제 후 값: {afterValue}");
    }
#endif

}
