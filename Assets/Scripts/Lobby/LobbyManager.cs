using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private LobbyController LobbyController; // 로비 패널 참조
    [SerializeField] private Image curtainImage; // 커튼 이미지 참조
    [SerializeField] private Vector2 curtainImagePosition = new Vector2(0, 0f); // 커튼 이미지 위치

    [Header("Scene Names")]
    [SerializeField] private string GameSceneName = "MainScene";

    private Sequence _seq;

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
}
