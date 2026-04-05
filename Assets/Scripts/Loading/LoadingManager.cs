using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    private const int LoadingCanvasSortingOrder = 32767;

    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private Text legacyProgressText;
    [SerializeField] private Text legacyTipText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Canvas loadingCanvas;
    [SerializeField] private string fallbackSceneName = "Title";
    [SerializeField] private float minimumLoadingDuration = 0.6f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float postSceneHoldDuration = 0.35f;
    [SerializeField] private int settleFrameCount = 2;
    [SerializeField]
    private string[] tips =
    {
        "카세트 테이프를 모아 다음 스테이지를 해금하세요.",
        "미니게임 전환 직후에는 입력을 조금만 천천히 해보세요.",
        "점수가 높을수록 다음 스테이지 개방이 빨라집니다.",
    };

    private bool sceneLoaded;

    private void Start()
    {
        AutoBindReferences();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(LoadRoutine());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

        CanvasGroup preloadedOverlay = SceneLoader.TakePreloadOverlay();
        if (preloadedOverlay != null)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            yield return FadeCanvas(preloadedOverlay, 1f, 0f, fadeInDuration);
            Destroy(preloadedOverlay.gameObject);
        }
        else
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            yield return null;
            yield return FadeCanvas(canvasGroup, 0f, 1f, fadeInDuration);
        }

        float startedAt = Time.unscaledTime;
        AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName);
        op.allowSceneActivation = false;
        sceneLoaded = false;

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

        while (!sceneLoaded)
        {
            yield return null;
        }

        for (int i = 0; i < settleFrameCount; i++)
        {
            yield return null;
        }

        if (postSceneHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(postSceneHoldDuration);
        }

        yield return FadeCanvas(canvasGroup, 1f, 0f, fadeOutDuration);
        Destroy(gameObject);
    }

    private void AutoBindReferences()
    {
        if (loadingCanvas == null)
        {
            loadingCanvas = GetComponent<Canvas>();
        }

        if (loadingCanvas != null)
        {
            loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            loadingCanvas.overrideSorting = true;
            loadingCanvas.sortingOrder = LoadingCanvasSortingOrder;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

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

    private IEnumerator FadeCanvas(CanvasGroup target, float from, float to, float duration)
    {
        if (target == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            target.alpha = to;
            yield break;
        }

        target.alpha = from;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        target.alpha = to;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoadingScene")
        {
            return;
        }

        sceneLoaded = true;
    }
}
