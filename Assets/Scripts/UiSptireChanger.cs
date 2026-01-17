using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiSpriteChanger : MonoBehaviour
{
    [System.Serializable]
    public class UiStyle
    {
        public Sprite pauseSprite;
        public Sprite timerSprite;

        public Sprite buttonSprite;
        public TMP_FontAsset fontStyle;
    }
    [Header("스타일")]
    [SerializeField] private UiStyle flappyStyle;
    [SerializeField] private UiStyle anipangStyle;

    [Header("일반 화면 UI")]

    [SerializeField] private Image pauseImage;

    [SerializeField] private TextMeshProUGUI scoreText;

    [SerializeField] private Image timerImage;

    [Header("퍼즈 화면 UI")]

    [SerializeField] private Image resumeImage;

    [SerializeField] private Image restartImage;

    [SerializeField] private Image restartImage2;

    [SerializeField] private Image settingImage;

    [SerializeField] private Image quitImage;

    [SerializeField] private Image quitImage2;
    [SerializeField] private TextMeshProUGUI resumeText;

    [SerializeField] private TextMeshProUGUI restartText;

    [SerializeField] private TextMeshProUGUI restartText2;

    [SerializeField] private TextMeshProUGUI settingText;

    [SerializeField] private TextMeshProUGUI quitText;
    [SerializeField] private TextMeshProUGUI quitText2;


    [SerializeField] private TextMeshProUGUI gemeoverText;

    [SerializeField] private TextMeshProUGUI alarmText;


    private int currentStyleIndex = 0;


    private void Start()
    {
        // GameSceneManager의 이벤트에 구독
        GameSceneManager.Instance.OnGameChanged += ChangeUiStyle;
        GameSceneManager.Instance.OnPausePanelOpened += ApplyUiStyle;
    }

    private void OnDestroy()
    {
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.OnGameChanged -= ChangeUiStyle;
            GameSceneManager.Instance.OnPausePanelOpened -= ApplyUiStyle;
        }
    }

    private void ChangeUiStyle()
    {
        currentStyleIndex = (currentStyleIndex + 1) % 2;
        ApplyUiStyle();

    }
    private void ApplyUiStyle()
    {

        UiStyle currentStyle = currentStyleIndex == 0 ? anipangStyle : flappyStyle;

        // 이미지 스타일 변경
        if (currentStyle.pauseSprite != null && currentStyle.timerSprite != null && currentStyle.buttonSprite != null)
        {
            pauseImage.sprite = currentStyle.pauseSprite;
            timerImage.sprite = currentStyle.timerSprite;
            resumeImage.sprite = currentStyle.buttonSprite;
            restartImage.sprite = currentStyle.buttonSprite;
            restartImage2.sprite = currentStyle.buttonSprite;
            settingImage.sprite = currentStyle.buttonSprite;
            quitImage.sprite = currentStyle.buttonSprite;
            quitImage2.sprite = currentStyle.buttonSprite;
        }

        // 텍스트 스타일 변경
        if (currentStyle.fontStyle != null)
        {
            scoreText.font = currentStyle.fontStyle;
            resumeText.font = currentStyle.fontStyle;
            restartText.font = currentStyle.fontStyle;
            restartText2.font = currentStyle.fontStyle;
            settingText.font = currentStyle.fontStyle;
            quitText.font = currentStyle.fontStyle;
            quitText2.font = currentStyle.fontStyle;
            gemeoverText.font = currentStyle.fontStyle;
            alarmText.font = currentStyle.fontStyle;
        }
    }
}
