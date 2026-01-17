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

    public GameObject anipangUIPrefab;

    public GameObject flappyBirdUIPrefab;

    [Header("Game object")]
    public Image gameTimer; // [호환성] 단일 타이머 연결 (자동으로 gameTimers에 추가됨)
    public List<Image> gameTimers = new List<Image>(); // 모든 타이머 UI 이미지 연결
    [SerializeField] private TextMeshProUGUI penaltyText; // 페널티 텍스트

    public TextMeshProUGUI gameScore; // 게임 스코어 출력
    // public GameObject gameChangeButton; // gameChangeButton (단일 연결)


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
    public bool IsTransitioning => _isTransitioning;
    private Vector3 _penaltyTextOriginPos;

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
        // [호환성] gameTimer가 연결되어 있다면 리스트에 추가
        if (gameTimer != null && !gameTimers.Contains(gameTimer))
        {
            gameTimers.Add(gameTimer);
        }

        if (penaltyText != null)
        {
            _penaltyTextOriginPos = penaltyText.transform.localPosition;
            penaltyText.gameObject.SetActive(false);
        }

        ResetGame(); // 게임 초기화

        // gameChangeButton.SetActive(false); // 시작 (플러피 버드에서 비활성화)
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
        currentTime += 5f; // 초기화 한정 패널티 무효

        float fill = (gameTimeLimit > 0f) ? (currentTime / gameTimeLimit) : 0f;
        foreach (var timer in gameTimers)
        {
            if (timer != null) timer.fillAmount = Mathf.Clamp01(fill);
        }

        GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.FlappyBird); // 플래피버드 BGM 재생

    }
    void Update()
    {
        if (isGameOver) return;
        if (isPaused) return;

        currentTime -= Time.deltaTime; // 시간 감소

        float fill = (gameTimeLimit > 0f) ? (currentTime / gameTimeLimit) : 0f;
        foreach (var timer in gameTimers)
        {
            if (timer != null) timer.fillAmount = Mathf.Clamp01(fill);
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

    public void ResumeGame()
    {
        isPaused = false;
    }

    /// <summary>
    /// 시각적 연출(Glitch/Fade)과 함께 게임 오브젝트를 교체하는 코루틴입니다.
    /// </summary>
    private IEnumerator ChangeGameRoutine()
    {
        _isTransitioning = true;
        isPaused = true; // 게임 로직(타이머 등) 일시 정지

        // -----------------------------------------------------------
        // 0. (AniPang -> FlappyBird) 페널티 연출 (전환 전)
        // -----------------------------------------------------------
        if (currentGameId == 0) // Present -> Past
        {
            if (penaltyText != null)
            {
                penaltyText.gameObject.SetActive(true);
                penaltyText.text = "-5s";
                penaltyText.transform.localPosition = _penaltyTextOriginPos;
                penaltyText.alpha = 1f;

                penaltyText.transform.DOLocalMoveY(_penaltyTextOriginPos.y + 100f, 0.8f).SetEase(Ease.OutQuad);
                penaltyText.DOFade(0f, 0.8f).SetEase(Ease.InQuad);
            }

            if (gameTimers != null && gameTimers.Count > 0)
            {
                float targetTime = Mathf.Max(0, currentTime - 5f);
                float targetFill = (gameTimeLimit > 0f) ? (targetTime / gameTimeLimit) : 0f;

                Sequence penaltySeq = DOTween.Sequence();
                foreach (var timer in gameTimers)
                {
                    if (timer != null) penaltySeq.Join(timer.DOFillAmount(targetFill, 0.8f));
                }
                yield return penaltySeq.WaitForCompletion();
            }
            else
            {
                yield return new WaitForSeconds(0.8f);
            }

            currentTime -= 5f;
        }

        OnGameChanged?.Invoke();

        // -----------------------------------------------------------
        // 1. 전환 시작 연출 (화면 암전 + 글리치 시작)
        // -----------------------------------------------------------
        if (transitionVisuals is not null)
        {
            // 시작할 때는 Volume을 켜고 -> 글리치를 실행
            // (필요하다면 여기도 Sequence로 묶을 수 있습니다)
            transitionVisuals.FadePastVolumeWeight(1f);
            transitionVisuals.FadePresentVolumeWeight(0f);
            
            Tween startTween = transitionVisuals.PlayStartMixedGlitch(0.5f, 1.0f);
            if (startTween != null) yield return startTween.WaitForCompletion();
        }

        // -----------------------------------------------------------
        // 2. 게임 오브젝트(프리팹) 교체 로직
        // -----------------------------------------------------------
        if (currentGameId == 1) // 현재 Flappy -> Anipang으로 전환
        {
            anipangPrefab.SetActive(true);
            flappyBirdPrefab.SetActive(false);

            anipangUIPrefab.SetActive(true);
            flappyBirdUIPrefab.SetActive(false);

            currentGameId = 0;

            GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.Anipang);
        }
        else if (currentGameId == 0) // 현재 Anipang -> Flappy로 전환
        {
            anipangPrefab.SetActive(false);
            flappyBirdPrefab.SetActive(true);

            anipangUIPrefab.SetActive(false);
            flappyBirdUIPrefab.SetActive(true);


            currentGameId = 1;

            // currentTime -= 5f; // 페널티 이미 적용됨
            GameManager.Instance.soundManager.PlayBGM(SoundManager.BGM.FlappyBird);
        }

        // 잠시 대기
        yield return null;

        // -----------------------------------------------------------
        // 3. 전환 종료 연출 (수정된 부분: 동시 실행 로직 적용)
        // -----------------------------------------------------------
        if (transitionVisuals is not null)
        {
            // 새로운 시퀀스를 생성하여 애니메이션들을 병렬(Join)로 연결합니다.
            Sequence endSequence = DOTween.Sequence();

            // A. 글리치 효과 종료 애니메이션 추가
            endSequence.Join(transitionVisuals.PlayEndMixedGlitch());

            // B. '애니팡(0)'으로 왔을 때만 Volume 비활성화 애니메이션을 '동시에' 추가
            if (currentGameId == 0)
            {
                // 앞서 수정한 TransitionVisuals 덕분에 SetVolumeActive가 Tween을 반환하므로 Join 가능
                endSequence.Join(transitionVisuals.FadePastVolumeWeight(0f));
                endSequence.Join(transitionVisuals.FadePresentVolumeWeight(1f));

            }

            // 두 애니메이션이 동시에 진행되고 끝날 때까지 대기
            yield return endSequence.WaitForCompletion();
        }

        _isTransitioning = false;

        if (currentGameId == 0)
        {
            isPaused = false;
        }
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

