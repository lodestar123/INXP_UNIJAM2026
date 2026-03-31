using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private Text legacyProgressText;
    [SerializeField] private Text legacyTipText;
    [SerializeField] private string fallbackSceneName = "Title";
    [SerializeField] private float minimumLoadingDuration = 0.6f;
    [SerializeField] private string[] tips =
    {
        "카세트 테이프를 모아 다음 스테이지를 해금하세요.",
        "미니게임 전환 직후에는 입력을 조금만 천천히 해보세요.",
        "점수가 높을수록 다음 스테이지 개방이 빨라집니다.",
    };

    private void Start()
    {
        AutoBindReferences();
        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        string targetSceneName = SceneLoader.NextSceneName;

        if (string.IsNullOrWhiteSpace(targetSceneName) && !string.IsNullOrWhiteSpace(fallbackSceneName))
        {
            targetSceneName = fallbackSceneName;
            Debug.LogWarning($"[LoadingManager] NextSceneName is empty. Falling back to '{fallbackSceneName}'.");
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("[LoadingManager] NextSceneName is empty.");
            SetProgressLabel("Load Failed");

            yield break;
        }

        if (tipText != null && tips != null && tips.Length > 0)
        {
            SetTipLabel(tips[Random.Range(0, tips.Length)]);
        }
        else if (legacyTipText != null && tips != null && tips.Length > 0)
        {
            SetTipLabel(tips[Random.Range(0, tips.Length)]);
        }

        float startedAt = Time.unscaledTime;
        AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            if (progressBar != null) progressBar.fillAmount = progress;

            SetProgressLabel($"{progress * 100f:0}%");

            if (op.progress >= 0.9f)
            {
                float remainingTime = minimumLoadingDuration - (Time.unscaledTime - startedAt);
                if (remainingTime > 0f)
                {
                    yield return new WaitForSecondsRealtime(remainingTime);
                }

                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private void AutoBindReferences()
    {
        if (progressBar == null)
        {
            Transform fillTransform = transform.Find("Content/ProgressBar/Fill");
            if (fillTransform != null)
            {
                progressBar = fillTransform.GetComponent<Image>();
            }
        }

        if (progressText == null)
        {
            Transform textTransform = transform.Find("Content/ProgressText");
            if (textTransform != null)
            {
                progressText = textTransform.GetComponent<TextMeshProUGUI>();
                legacyProgressText = textTransform.GetComponent<Text>();
            }
        }
        else if (legacyProgressText == null)
        {
            legacyProgressText = progressText.GetComponent<Text>();
        }

        if (tipText == null)
        {
            Transform tipTransform = transform.Find("Content/TipText");
            if (tipTransform != null)
            {
                tipText = tipTransform.GetComponent<TextMeshProUGUI>();
                legacyTipText = tipTransform.GetComponent<Text>();
            }
        }
        else if (legacyTipText == null)
        {
            legacyTipText = tipText.GetComponent<Text>();
        }
    }

    private void SetProgressLabel(string value)
    {
        if (progressText != null)
        {
            progressText.text = value;
        }

        if (legacyProgressText != null)
        {
            legacyProgressText.text = value;
        }
    }

    private void SetTipLabel(string value)
    {
        if (tipText != null)
        {
            tipText.text = value;
        }

        if (legacyTipText != null)
        {
            legacyTipText.text = value;
        }
    }
}
