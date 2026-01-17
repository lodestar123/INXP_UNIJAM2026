using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Utils;
using DG.Tweening;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }
    
    [SerializeField] private TransitionVisuals transitionVisuals;
    
    [Header("GamePrefabs")] // 각 게임 전체 화면 프리팹, 연결 필수!!
    public GameObject anipangPrefab;

    public GameObject flappyBirdPrefab;

    [Header("Game object")]
    public Image gameTimer; // 타이머 UI 이미지 연결

    public TextMeshProUGUI gameScore; // 게임 스코어 출력
    public GameObject gameChangeButton; // gameChangeButton


    [Header("Game State")]
    private List<int> collectedItems = new List<int>(); // 수집한 아이템 id를 차례로 저장

    public IReadOnlyList<int> CollectedItems => collectedItems;
    public int CurrentScore { get; private set; } // 현재 점수
    [SerializeField] private float currentTime; // 남은 시간 카운트
    public float CurrentTime => currentTime;


    private bool isGameOver = false; // 게임 오버 여부
    public bool IsGameOver => isGameOver;
    private bool isPaused = false; // 게임 일시정지 여부
    public bool IsPaused => isPaused;
    private int currentGameId = 0; // 현재 게임 ID (0: 애니팡, 1: 플래피버드)
    public int CurrentGameId => currentGameId;
    
    private bool _isTransitioning = false;

    public event System.Action OnGameChanged;

    public event System.Action OnPausePanelOpened;
    public event System.Action OnGameOver;

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

        gameChangeButton.SetActive(false); // 시작 (플러피 버드에서 비활성화)
    }
    public void ResetGame() // 새 게임 시작 시 필요
    {
        CurrentScore = 0;
        gameScore.text = CurrentScore.ToString();

        currentTime = gameTimeLimit;
        collectedItems.Clear();
        isGameOver = false;
        isPaused = false;
        _isTransitioning = false;
        anipangPrefab.SetActive(false);
        flappyBirdPrefab.SetActive(false);
        currentGameId = 0; // 기본 현재 게임 애니팡 설정

        // 애니팡 큐 초기화
        if (ItemQueueManager.Instance != null)
        {
            ItemQueueManager.Instance.ClearQueue();
            GameManager.Instance.GameData.itemQueue = new ItemQueue();
        }
        
        // FlappyBirdGameManager 상태 초기화
        if (FlappyBird.Game.FlappyBirdGameManager.Instance != null)
        {
            FlappyBird.Game.FlappyBirdGameManager.Instance.ResetGameState();
        }
        
        OnChangeGame(); // 플러피 버드로 게임 변경

        if (gameTimer is not null)
        {
            float fill = (gameTimeLimit > 0f) ? (currentTime / gameTimeLimit) : 0f;
            gameTimer.fillAmount = Mathf.Clamp01(fill);
        }

        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.FlappyBird); // 플래피버드 BGM 재생

    }
    void Update()
    {
        if (isGameOver) return;
        if (isPaused) return;

        currentTime -= Time.deltaTime; // 시간 감소

        if (gameTimer is not null) // 타이머 UI 업데이트
        {
            float fill = (gameTimeLimit > 0f) ? (currentTime / gameTimeLimit) : 0f;
            gameTimer.fillAmount = Mathf.Clamp01(fill);
        }

        if (currentTime <= 0)
        {
            currentTime = 0;
            isGameOver = true;
            OnGameOver?.Invoke(); // 게임오버
            OnPausePanelOpened?.Invoke();
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

    public void AddScore(int score, bool forceAddScore = false) // 점수 추가
    {
        if (isGameOver) return;
        if (isPaused && !forceAddScore) return;

        CurrentScore += score;
        gameScore.text = CurrentScore.ToString();
    }

    /// <summary>
    /// 게임 전환을 요청하는 메서드입니다. 실제 로직은 코루틴에서 수행합니다.
    /// </summary>
    public void OnChangeGame()
    {
        if (isGameOver) return;
        if (_isTransitioning) return; // 이미 전환 중이면 무시

        StartCoroutine(ChangeGameRoutine());
    }

    /// <summary>
    /// 시각적 연출(Glitch/Fade)과 함께 게임 오브젝트를 교체하는 코루틴입니다.
    /// </summary>
    private IEnumerator ChangeGameRoutine()
    {
        _isTransitioning = true;
        isPaused = true; // 게임 로직(타이머 등) 일시 정지

        OnGameChanged?.Invoke();

        // 1. 전환 시작 연출 (화면 암전 + 글리치 시작)
        // _transitionVisuals가 연결되어 있다면 애니메이션 재생 및 대기
        if (transitionVisuals is not null)
        {
            transitionVisuals.SetVolumeActive(true);
            
            Tween startTween = transitionVisuals.PlayStartAnimation();
            if (startTween != null) yield return startTween.WaitForCompletion();
        }

        // 2. 게임 오브젝트(프리팹) 교체 로직 (기존 OnChangeGame 내부 로직)
        // 화면이 가려진 상태에서 실행되므로 플레이어는 교체 순간을 볼 수 없음
        if (currentGameId == 1) // 현재 Flappy -> Anipang으로 전환
        {
            anipangPrefab.SetActive(true);
            flappyBirdPrefab.SetActive(false);

            gameChangeButton.SetActive(true);

            currentGameId = 0;

            GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Anipang);
        }
        else if (currentGameId == 0) // 현재 Anipang -> Flappy로 전환
        {
            flappyBirdPrefab.SetActive(true);
            anipangPrefab.SetActive(false);

            gameChangeButton.SetActive(false);

            currentGameId = 1;

            currentTime -= 5f; // 페널티 적용
            GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.FlappyBird);
        }

        // 잠시 대기 (씬 로딩과 다르게 즉시 교체되지만, 연출의 템포를 위해 아주 짧게 대기 가능)
        yield return null; 

        // 3. 전환 종료 연출 (화면 밝아짐 + 글리치 종료)
        if (transitionVisuals is not null)
        {
            Tween endTween = transitionVisuals.PlayEndAnimation();
            if (endTween != null) yield return endTween.WaitForCompletion();

            // 4. 전환이 다 끝난 후, '애니팡(0)'이라면 Volume 비활성화
            if (currentGameId == 0)
            {
                transitionVisuals.SetVolumeActive(false);
            }
        }

        isPaused = false;       // 게임 재개
        _isTransitioning = false; // 전환 상태 해제
    }

    public void OnApplicationPause(bool pause)
    {
        if (pause) OnPausePanelOpened?.Invoke();
        isPaused = pause;
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

