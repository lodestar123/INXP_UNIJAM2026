using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SceneLoader
{
    public static string NextSceneName { get; private set; }

    private static SceneLoaderRunner runner;
    private static CanvasGroup preloadOverlay;

    public static void Load(string sceneName)
    {
        if (GameManager.Instance != null && sceneName == "MainScene")
        {
            GameManager.Instance.EnsureValidCurrentStage();
        }

        NextSceneName = sceneName;
        GetRunner().BeginLoad();
    }

    public static CanvasGroup TakePreloadOverlay()
    {
        CanvasGroup overlay = preloadOverlay;
        preloadOverlay = null;
        return overlay;
    }

    private static SceneLoaderRunner GetRunner()
    {
        if (runner != null)
        {
            return runner;
        }

        GameObject runnerObject = new GameObject("SceneLoaderRunner");
        Object.DontDestroyOnLoad(runnerObject);
        runner = runnerObject.AddComponent<SceneLoaderRunner>();
        return runner;
    }

    private static CanvasGroup CreatePreloadOverlay()
    {
        GameObject overlayObject = new GameObject("SceneLoaderOverlay");
        Object.DontDestroyOnLoad(overlayObject);

        Canvas canvas = overlayObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = short.MaxValue;

        overlayObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = overlayObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CanvasGroup group = overlayObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = true;
        group.interactable = false;

        GameObject panelObject = new GameObject("Panel");
        panelObject.transform.SetParent(overlayObject.transform, false);

        RectTransform panelTransform = panelObject.AddComponent<RectTransform>();
        panelTransform.anchorMin = Vector2.zero;
        panelTransform.anchorMax = Vector2.one;
        panelTransform.offsetMin = Vector2.zero;
        panelTransform.offsetMax = Vector2.zero;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.03f, 0.05f, 0.08f, 1f);

        return group;
    }

    private sealed class SceneLoaderRunner : MonoBehaviour
    {
        [SerializeField] private float preLoadFadeDuration = 0.2f;

        private bool isLoading;

        public void BeginLoad()
        {
            if (isLoading)
            {
                return;
            }

            StartCoroutine(LoadRoutine());
        }

        private IEnumerator LoadRoutine()
        {
            isLoading = true;

            if (preloadOverlay == null)
            {
                preloadOverlay = CreatePreloadOverlay();
            }

            yield return Fade(preloadOverlay, 0f, 1f, preLoadFadeDuration);
            SceneManager.LoadScene("LoadingScene");

            isLoading = false;
        }

        private static IEnumerator Fade(CanvasGroup target, float from, float to, float duration)
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
    }
}
