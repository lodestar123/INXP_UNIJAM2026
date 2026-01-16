using UnityEngine;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    private List<int> collectedItems = new List<int>(); // 수집한 아이템 id를 차례로 저장

    public IReadOnlyList<int> CollectedItems => collectedItems;
    public int CurrentScore { get; private set; } // 현재 점수
    public float CurrentTime { get; private set; } // 남은 시간 카운트

    private bool isGameOver = false; // 게임 오버 여부
    public bool IsGameOver => isGameOver;
    private bool isPaused = false; // 게임 일시정지 여부

    [SerializeField] private float gameTimeLimit = 60f; // 게임 제한 시간

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    void Start()
    {
        ResetGame(); // 게임 초기화
    }
    public void ResetGame() // 새 게임 시작 시 필요
    {
        CurrentScore = 0;
        CurrentTime = gameTimeLimit;
        collectedItems.Clear();
        isGameOver = false;
        isPaused = false;
    }
    void Update()
    {
        if (isGameOver) return;
        if (isPaused) return;

        CurrentTime -= Time.deltaTime;

        if (CurrentTime <= 0)
        {
            CurrentTime = 0;
            OnGameOver(); // 게임오버
        }

    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddItem(int itemId) // 아이템 id 매개변수로 받아 리스트에 추가
    {
        if (isGameOver) return;
        if (isPaused) return;
        collectedItems.Add(itemId);
    }
    public void ClearItem() // 아이템 전체 삭제
    {
        if (isGameOver) return;
        if (isPaused) return;
        collectedItems.Clear();
    }

    public void AddScore(int score) // 점수 추가
    {
        if (isGameOver) return;
        if (isPaused) return;
        CurrentScore += score;
    }

    void OnGameOver()
    {
        if (isGameOver) return;

        isGameOver = true; // 게임 오버

        // 최종 점수 비교 전달
        if (GameManager.Instance != null && GameManager.Instance.GameData != null)
        {
            if (CurrentScore > GameManager.Instance.GameData.highScore)
            {
                GameManager.Instance.GameData.highScore = CurrentScore;
            }
        }
    }
#if UNITY_EDITOR
    [Header("Test Settings")]
    [SerializeField] private int testItemId = 1;

    [ContextMenu("Test: Add Item (Use Test Item ID)")]
    void TestAddItem()
    {
        AddItem(testItemId);
        Debug.Log($"Item {testItemId} added! Total: {CollectedItems.Count}");
    }

    [ContextMenu("Test: Add 5 Random Items")]
    void TestAddRandomItems()
    {
        for (int i = 0; i < 5; i++)
        {
            int randomId = Random.Range(0, 5);
            AddItem(randomId);
        }
        Debug.Log($"5 random items added! Total: {CollectedItems.Count}");
    }
    [ContextMenu("Test: Add 100 Score")]
    void TestAddScore()
    {
        AddScore(100);
        Debug.Log($"Score added! Current: {CurrentScore}");
    }

    [ContextMenu("Test: Trigger Game Over")]
    void TestGameOver()
    {
        OnGameOver();
    }

    [ContextMenu("Debug: Print Current State")]
    void DebugPrintState()
    {
        Debug.Log($"Score: {CurrentScore}, Time: {CurrentTime:F1}, GameOver: {isGameOver}, Paused: {isPaused}");
    }

    [ContextMenu("Debug: Print Current Items")]
    void DebugPrintItems()
    {
        Debug.Log($"Collected Items: {string.Join(", ", CollectedItems)}");
    }
#endif
}

