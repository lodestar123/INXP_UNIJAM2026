using UnityEngine;
using TMPro;
using DG.Tweening;

public class ItemCounter : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI countText;

    [Header("설정")]
    [SerializeField] private int totalCells = 49; // 큐 최대치(maxQueueSize)와 동일하게

    [Header("깜박임 연출")]
    [SerializeField] private float blinkDuration = 0.2f;
    [SerializeField] private Color blinkColor = Color.yellow;

    private int _lastQueued = -1;
    private Color _baseColor = Color.white;

    void Awake()
    {
        if (countText != null) _baseColor = countText.color;
    }

    void OnEnable()
    {
        _lastQueued = -1;     // 다시 켜질 때 첫 프레임은 깜박임 없이 표기만
        Refresh(blink: false); // 현재 큐 개수로 즉시 초기화
    }

    void LateUpdate()
    {
        Refresh(blink: true);
    }

    private void Refresh(bool blink)
    {
        int queued = (ItemQueueManager.Instance != null) ? ItemQueueManager.Instance.ItemCount : 0;
        int remaining = Mathf.Max(0, totalCells - queued);
        if (countText != null) countText.text = remaining.ToString();

        // 큐가 늘었을 때(= 아이템을 먹었을 때)만 깜박임. 보드 채우기로 줄 땐 제외.
        if (blink && _lastQueued >= 0 && queued > _lastQueued)
            PlayBlink();
        _lastQueued = queued;
    }

    private void PlayBlink()
    {
        if (countText == null) return;

        countText.transform.DOKill();
        countText.DOKill();
        countText.color = _baseColor;

        float half = blinkDuration * 0.5f;
        Sequence seq = DOTween.Sequence();
        seq.Join(countText.DOColor(blinkColor, half));
        seq.Join(countText.DOColor(_baseColor, half));
        seq.SetUpdate(true);
    }
}