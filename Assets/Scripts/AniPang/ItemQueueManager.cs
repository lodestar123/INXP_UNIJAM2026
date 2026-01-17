using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전역 아이템 큐를 관리하는 싱글톤 매니저
/// 게임 전체에서 사용되는 아이템 큐를 중앙에서 관리합니다.
/// </summary>
public class ItemQueueManager : MonoBehaviour 
{
    public static ItemQueueManager Instance { get; private set; }

    [SerializeField] private ItemQueue _itemQueue = new ItemQueue();
    
    [Header("Warning Settings")]
    [SerializeField] private int warningThreshold = 49;

    /// <summary>
    /// 현재 아이템 큐에 접근
    /// </summary>
    public ItemQueue Queue => _itemQueue;

    /// <summary>
    /// 큐에 저장된 아이템 개수
    /// </summary>
    public int ItemCount => _itemQueue.Count;

    /// <summary>
    /// 경고 패널 표시 이벤트 (외부에서 구독 가능)
    /// </summary>
    public event System.Action OnWarningThresholdReached;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// GameData에서 ItemQueue를 로드 (State Persistence용)
    /// </summary>
    private void LoadFromGameData()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameData != null)
        {
            // GameData의 itemQueue를 복사
            var gameDataQueue = GameManager.Instance.GameData.itemQueue;
            if (gameDataQueue != null && gameDataQueue.Count > 0)
            {
                // 직렬화된 데이터를 복원
                gameDataQueue.Deserialize();
                _itemQueue = gameDataQueue;
                Debug.Log($"[ItemQueueManager] GameData에서 {_itemQueue.Count}개 아이템 로드됨");
            }
            else
            {
                // GameData에 itemQueue가 없으면 새로 생성
                _itemQueue.Deserialize();
            }
        }
        else
        {
            // GameManager가 없으면 기본 복원
            _itemQueue.Deserialize();
        }
    }

    /// <summary>
    /// GameData에 ItemQueue를 저장합니다
    /// </summary>
    public void SaveToGameData()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameData != null)
        {
            _itemQueue.Serialize();
            GameManager.Instance.GameData.itemQueue = _itemQueue;
            Debug.Log($"[ItemQueueManager] GameData에 {_itemQueue.Count}개 아이템 저장됨");
        }
    }

    /// <summary>
    /// 아이템을 큐에 추가 (append)
    /// </summary>
    public void AddItem(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemQueueManager] null 아이템은 추가할 수 없습니다.");
            return;
        }

        _itemQueue.Enqueue(item);
        CheckWarningThreshold();
    }

    /// <summary>
    /// 여러 아이템을 큐에 추가 (append)
    /// </summary>
    public void AddItems(IEnumerable<Item> items)
    {
        if (items == null) return;
        _itemQueue.EnqueueRange(items);
        CheckWarningThreshold();
    }

    /// <summary>
    /// 경고 임계값(49개) 체크 및 이벤트 발생
    /// </summary>
    private void CheckWarningThreshold()
    {
        if (_itemQueue.Count >= warningThreshold)
        {
            Debug.Log($"[ItemQueueManager] 경고: 아이템 큐에 {_itemQueue.Count}개가 쌓였습니다! (임계값: {warningThreshold})");
            
            OnWarningThresholdReached?.Invoke();
        }
    }

    /// <summary>
    /// 큐에서 지정된 개수만큼 아이템을 가져옴 (제거하지 않음)
    /// </summary>
    public List<Item> PeekItems(int count)
    {
        return _itemQueue.Peek(count);
    }

    /// <summary>
    /// 큐에서 지정된 개수만큼 아이템을 제거하고 반환
    /// </summary>
    public List<Item> RemoveItems(int count)
    {
        return _itemQueue.Dequeue(count);
    }

    /// <summary>
    /// 큐의 모든 아이템을 반환 (복사본)
    /// </summary>
    public List<Item> GetAllItems()
    {
        return _itemQueue.GetAllItems();
    }

    /// <summary>
    /// 큐 초기화
    /// </summary>
    public void ClearQueue()
    {
        _itemQueue.Clear();
    }

    /// <summary>
    /// 직렬화를 위해 큐 상태를 저장
    /// </summary>
    public void Serialize()
    {
        _itemQueue.Serialize();
    }

    /// <summary>
    /// 직렬화된 데이터를 복원
    /// </summary>
    public void Deserialize()
    {
        _itemQueue.Deserialize();
    }

    /// <summary>
    /// 디버그용: 현재 큐 상태 출력
    /// </summary>
    [ContextMenu("Debug: Print Queue State")]
    public void DebugPrintQueue()
    {
        _itemQueue.DebugPrint();
    }

#if UNITY_EDITOR
    [Header("Test Settings")]
    [SerializeField] private Item testItem;
    [SerializeField] private int testItemCount = 5;

    [ContextMenu("Test: Add Test Item")]
    void TestAddItem()
    {
        if (testItem != null)
        {
            AddItem(testItem);
        }
        else
        {
            Debug.LogWarning("[ItemQueueManager] 테스트 아이템이 설정되지 않았습니다.");
        }
    }

    [ContextMenu("Test: Add Random Items")]
    void TestAddRandomItems()
    {
        if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0)
        {
            Debug.LogWarning("[ItemQueueManager] ItemDataBase에 아이템이 없습니다.");
            return;
        }

        for (int i = 0; i < testItemCount; i++)
        {
            int randomIndex = Random.Range(0, ItemDataBase.Items.Length);
            AddItem(ItemDataBase.Items[randomIndex]);
        }
    }

    [ContextMenu("Test: Clear Queue")]
    void TestClearQueue()
    {
        ClearQueue();
    }
#endif
}
