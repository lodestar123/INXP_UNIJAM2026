using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 애니팡 콤보 카운트, 제한 시간, UI, 점수 배율을 관리
/// </summary>
public class ComboManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI comboText;

    [Header("콤보 시간")]
    [SerializeField] private float maxComboTime = 3f;

    [Header("텍스트 애니메이션")]
    [SerializeField] private float popScaleMultiplier = 1.3f;
    [SerializeField] private float popScaleUpDuration = 0.15f;
    [SerializeField] private float popScaleDownDuration = 0.15f;

    private int _comboCount;
    private float _comboTimeRemaining;
    private float _decayAccumulator;
    private Vector3 _comboTextBaseScale = Vector3.one;

    public int ComboCount => _comboCount;
    public bool IsActive => _comboCount > 0;
    public float MaxComboTime => maxComboTime;

    private void Awake()
    {
        CacheComboTextBaseScale();
        HideComboTextImmediate();
    }

    private void OnDisable()
    {
        ResetCombo();
    }

    private void Update()
    {
        if (!IsActive) return;
        if (IsGamePaused()) return;

        _decayAccumulator += Time.deltaTime;

        while (_decayAccumulator >= 1f)
        {
            _decayAccumulator -= 1f;
            _comboTimeRemaining -= 1f;

            if (_comboTimeRemaining <= 0f)
            {
                ResetCombo();
                return;
            }
        }
    }

    /// <summary>
    /// 플레이어 스왑으로 매치가 성공했을 때 호출
    /// </summary>
    public void RegisterPlayerPop()
    {
        _comboCount++;
        _comboTimeRemaining = maxComboTime;
        _decayAccumulator = 0f;

        if (_comboCount >= 2)
        {
            ShowComboTextWithPopAnimation();
        }
        else
        {
            HideComboTextImmediate();
        }
    }

    /// <summary>
    /// N ≥ 2일 때 1 + 0.N 배, 그 외 1배
    /// </summary>
    public float GetScoreMultiplier()
    {
        if (_comboCount < 2) return 1f;
        return 1f + _comboCount * 0.1f;
    }

    public void ResetCombo()
    {
        _comboCount = 0;
        _comboTimeRemaining = 0f;
        _decayAccumulator = 0f;
        HideComboTextImmediate();
    }

    private void ShowComboTextWithPopAnimation()
    {
        if (comboText == null) return;

        comboText.gameObject.SetActive(true);
        comboText.text = $"{_comboCount} COMBO";

        Transform textTransform = comboText.transform;
        textTransform.DOKill();
        textTransform.localScale = _comboTextBaseScale;

        DOTween.Sequence()
            .Append(textTransform.DOScale(_comboTextBaseScale * popScaleMultiplier, popScaleUpDuration).SetEase(Ease.OutBack))
            .Append(textTransform.DOScale(_comboTextBaseScale, popScaleDownDuration).SetEase(Ease.InOutQuad));
    }

    private void HideComboTextImmediate()
    {
        if (comboText == null) return;

        comboText.transform.DOKill();
        comboText.transform.localScale = _comboTextBaseScale;
        comboText.gameObject.SetActive(false);
    }

    private void CacheComboTextBaseScale()
    {
        if (comboText == null) return;
        _comboTextBaseScale = comboText.transform.localScale;
    }

    private static bool IsGamePaused()
    {
        return GameSceneManager.Instance != null && GameSceneManager.Instance.IsPaused;
    }
}
