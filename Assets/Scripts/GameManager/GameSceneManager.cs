using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    [Header("GamePrefabs")] // 각 게임 전체 화면 프리팹, 연결 필수!!
    public GameObject anipangPrefab;

    public GameObject flappyBirdPrefab;

    [Header("Game object")]
    public Image gameTimer; // 타이머 UI 이미지 연결
    public GameObject gameOverPanel; // 게임 오버 패널
    public TextMeshProUGUI gameResult; // 게임 결과 출력


    [Header("Game State")]
    private List<int> collectedItems = new List<int>(); // 수집한 아이템 id를 차례로 저장

    public IReadOnlyList<int> CollectedItems => collectedItems;
    public int CurrentScore { get; private set; } // 현재 점수
    public float CurrentTime { get; private set; } // 남은 시간 카운트

    private bool isGameOver = false; // 게임 오버 여부
    public bool IsGameOver => isGameOver;
    private bool isPaused = false; // 게임 일시정지 여부
    public bool IsPaused => isPaused;
    private int currentGameId = 0; // 현재 게임 ID (0: 애니팡, 1: 플래피버드)
    public int CurrentGameId => currentGameId;


    [SerializeField] private float gameTimeLimit = 60f; // 게임 제한 시간
    private enum GamePrefabState // 현재 게임 상태
    {
        anipang,
        flappyBird
    }

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
        anipangPrefab.SetActive(false);
        flappyBirdPrefab.SetActive(false);
        currentGameId = 0; // 기본 현재 게임 애니팡 설정
        OnChangeGame(); // 플러피 버드로 게임 변경

        if (gameTimer is not null)
        {
            float fill = (gameTimeLimit > 0f) ? (CurrentTime / gameTimeLimit) : 0f;
            gameTimer.fillAmount = Mathf.Clamp01(fill);
        }

        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.FlappyBird); // 플래피버드 BGM 재생

    }
    void Update()
    {
        if (isGameOver) return;
        if (isPaused) return;

        CurrentTime -= Time.deltaTime; // 시간 감소

        if (gameTimer is not null) // 타이머 UI 업데이트
        {
            float fill = (gameTimeLimit > 0f) ? (CurrentTime / gameTimeLimit) : 0f;
            gameTimer.fillAmount = Mathf.Clamp01(fill);
        }

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

    public void OnChangeGame() // 게임 전환
    {
        if (isGameOver) return;
        if (isPaused) return;

        isPaused = true; // 게임 전환 연출 중 타임 변화 정지

        // 연출 관련 함수 추가?

        if (currentGameId == 1)
        {
            anipangPrefab.SetActive(true);
            flappyBirdPrefab.SetActive(false);
            currentGameId = 0;

            GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Anipang); // 애니팡 BGM 재생

        }
        else if (currentGameId == 0)
        {
            flappyBirdPrefab.SetActive(true);
            anipangPrefab.SetActive(false);

            currentGameId = 1;

            CurrentTime -= 5f; // 5초 패널티 발생
            GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.FlappyBird); // 플래피버드 BGM 재생

        }

        isPaused = false; // 타임 변화 허용
    }

    public void OnApplicationPause(bool pause)
    {
        isPaused = pause;
    }

    void OnGameOver()
    {
        if (isGameOver) return;

        isGameOver = true; // 게임 오버
        Time.timeScale = 0f;

        gameOverPanel.SetActive(true);
        gameResult.text = CurrentScore.ToString();

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

    [SerializeField] private int testItem = 5;

    [ContextMenu("Test: Add Item (Use Test Item ID)")]
    void TestAddItem()
    {
        AddItem(testItemId);
        Debug.Log($"Item {testItemId} added! Total: {CollectedItems.Count}");
    }

    [ContextMenu("Test: Add testItem Random Items")]
    void TestAddRandomItems()
    {
        for (int i = 0; i < testItem; i++)
        {
            int randomId = Random.Range(0, 5);
            AddItem(randomId);
        }
        Debug.Log($"{testItem} random items added! Total: {CollectedItems.Count}");
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

