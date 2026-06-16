using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class LobbyStageButtonVisual
{
    [Tooltip("스테이지 버튼 이미지")]
    public Image buttonImage;

    [Tooltip("클리어 시 표시할 스프라이트")]
    public Sprite clearedSprite;
}

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private LobbyController LobbyController; // 로비 패널 참조
    [SerializeField] private Image curtainImage; // 커튼 이미지 참조
    [SerializeField] private Vector2 curtainImagePosition = new Vector2(0, 0f); // 커튼 이미지 위치

    [Header("Stage Button Visuals")]
    [SerializeField] private List<LobbyStageButtonVisual> stageButtonVisuals;

    [Header("Scene Names")]
    [SerializeField] private string GameSceneName = "MainScene";
    [SerializeField] private string ReViewSceneName = "ReViewScene";

    private Sequence _seq;
    private readonly List<Sprite> _defaultStageButtonSprites = new List<Sprite>();

    private void Awake()
    {
        CacheDefaultStageButtonSprites();
    }

    public void Start()
    {
        Time.timeScale = 1f;
        _seq?.Kill();

        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Title);

        curtainImage.rectTransform.anchoredPosition = new Vector2(0, 0); // 커튼 이미지 초기 위치 설정
        curtainImagePosition = new Vector2(0, 0); // 커튼 이미지 적용할 포지션 초기화

        for (int i = 0; i < GameManager.Instance.GameData.stageUnlocked.Count; i++)
        {
            if (i != 0 && GameManager.Instance.GameData.stageUnlocked[i])
            {
                curtainImagePosition += new Vector2(400f, 0); // 커튼 이미지 적용할 포지션
            }
            else if (i != 0)
            {
                break;
            }
        }
        _seq.Join(curtainImage.rectTransform.DOAnchorPos(curtainImagePosition, 2f).SetEase(Ease.OutExpo));

        RefreshStageButtonSprites();
    }

    /// <summary>
    /// 로비 진입 시 스테이지 버튼 이미지를 클리어 여부에 맞게 갱신한다.
    /// </summary>
    public void RefreshStageButtonSprites()
    {
        if (GameManager.Instance == null) return;

        for (int i = 0; i < stageButtonVisuals.Count; i++)
        {
            LobbyStageButtonVisual visual = stageButtonVisuals[i];
            if (visual == null || visual.buttonImage == null) continue;

            if (GameManager.Instance.IsStageCleared(i) && visual.clearedSprite != null)
            {
                visual.buttonImage.sprite = visual.clearedSprite;
            }
            else if (i < _defaultStageButtonSprites.Count && _defaultStageButtonSprites[i] != null)
            {
                visual.buttonImage.sprite = _defaultStageButtonSprites[i];
            }
        }
    }

    private void CacheDefaultStageButtonSprites()
    {
        _defaultStageButtonSprites.Clear();

        foreach (LobbyStageButtonVisual visual in stageButtonVisuals)
        {
            if (visual != null && visual.buttonImage != null)
            {
                _defaultStageButtonSprites.Add(visual.buttonImage.sprite);
            }
            else
            {
                _defaultStageButtonSprites.Add(null);
            }
        }
    }

    public void onStageButton(int stageIndex)
    {
        if (GameManager.Instance.GameData.stageUnlocked[stageIndex])
        {
            GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
            LobbyController.openStageSelectPanel(stageIndex);
        }
        else
        {
            Debug.Log("해금되지 않은 스테이지입니다.");
        }

    }

    public void onReViewButton()
    {
        GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
        SceneLoader.Load(ReViewSceneName);
    }
}
