using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiSpriteChanger : MonoBehaviour
{
    [System.Serializable]
    public class UiStyle
    {
        public Sprite sprite1;
        public Sprite sprite2;
        public TMP_FontAsset fontStyle;
    }

    [SerializeField] private UiStyle flappyStyle;
    [SerializeField] private UiStyle anipangStyle;

    [SerializeField] private Image pauseImage;

    [SerializeField] private Image timerImage;

    [SerializeField] private TextMeshProUGUI scoreText;


    private int currentStyleIndex = 0;

    private void Start()
    {
        // GameSceneManager의 이벤트에 구독
        GameSceneManager.Instance.OnGameChanged += ChangeUiStyle;
    }

    private void OnDestroy()
    {
        if (GameSceneManager.Instance != null)
            GameSceneManager.Instance.OnGameChanged -= ChangeUiStyle;
    }

    private void ChangeUiStyle()
    {
        currentStyleIndex = (currentStyleIndex + 1) % 2;

        UiStyle currentStyle = currentStyleIndex == 0 ? anipangStyle : flappyStyle;

        pauseImage.sprite = currentStyle.sprite1;
        timerImage.sprite = currentStyle.sprite2;
        scoreText.font = currentStyle.fontStyle;
    }
}
