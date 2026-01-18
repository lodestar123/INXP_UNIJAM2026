using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Core.Input;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;

    [Header("테스트 설정")]
    [Tooltip("테스트 모드 활성화 시, 이 리스트의 아이템으로 보드를 생성합니다.")]
    [SerializeField] private bool useTestItems = false;
    [SerializeField] private List<Item> testItems = new List<Item>();
    
    [Header("보드 채우기 테스트 설정")]
    [Tooltip("테스트용 랜덤 아이템 개수 (보드 채우기 테스트)")]
    [SerializeField] private int testRandomItemCount = 30;
    
    [Tooltip("플러피→Match3 전환 테스트용 추가 랜덤 아이템 개수")]
    [SerializeField] private int testAdditionalRandomItemCount = 10;
    
    [Header("Pop 애니메이션 설정")]
    [Tooltip("팝 애니메이션의 목적지 오브젝트 (RectTransform)")]
    [SerializeField] private RectTransform popDestinationTarget;

    public Tile[,] Tiles { get; private set; }
    
    /// <summary>
    /// 팝 애니메이션의 목적지 RectTransform
    /// </summary>
    public RectTransform PopDestinationTarget => popDestinationTarget;

    public int width => Tiles.GetLength(0);
    public int height => Tiles.GetLength(1);

    // 핸들러 클래스들
    private MatchDetector _matchDetector;
    private TileSwapper _tileSwapper;
    private PopHandler _popHandler;
    private GravityHandler _gravityHandler;
    private BoardFillSystem _boardFillSystem;
    private BoardFillCursor _fillCursor;

    // UnifiedInputManager 참조
    private IUnifiedInput _inputManager;
    
    // Pop 처리 중 입력 차단 플래그
    private bool _isProcessing = false;
    
    // 스와이프 조작용 변수
    private Tile _swipeTile = null; // 스와이프 시작 타일
    private Vector2 _swipeStartPos; // 스와이프 시작 위치
    private bool _isDragging = false; // 드래그 중 여부
    private const float SwipeThreshold = 30f; // 스와이프 인식 최소 거리

    private void Awake() => Instance = this;

    private void Start()
    {
        Screen.SetResolution(2340, 1080, true);

        // 보드 크기 초기화
        int boardWidth = rows.Max(row => row.tiles.Length);
        int boardHeight = rows.Length;
        Tiles = new Tile[boardWidth, boardHeight];

        // 핸들러 초기화
        _matchDetector = new MatchDetector(Tiles);
        _gravityHandler = new GravityHandler(Tiles);
        _popHandler = new PopHandler(Tiles, _matchDetector, _gravityHandler);
        _tileSwapper = new TileSwapper(Tiles, _matchDetector, _popHandler, this);

        // BoardFillSystem 초기화
        InitializeBoardFillSystem();

        // 기획에 따라: ItemQueue를 기반으로 보드를 처음부터 채움
        // 순서 보존, 보정 없음,항상 처음부터 채우기
        if (_boardFillSystem != null)
        {
            InitializeBoardAsync(); // async 메서드 호출
        }
        else
        {
            Debug.LogError("[Board] BoardFillSystem 초기화 실패. 보드를 채울 수 없습니다.");
        }

        // UnifiedInputManager 초기화
        _inputManager = UnifiedInputManager.Instance;
    }
    
    /// <summary>
    /// 보드 초기화를 비동기로 수행
    /// </summary>
    private async void InitializeBoardAsync()
    {
        if (_boardFillSystem != null)
        {
            // 초기 매치 제거 중 입력 차단
            _isProcessing = true;
            try
            {
                await _boardFillSystem.FillBoardFromStart();
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }

    private void OnEnable()
    {
        if (_boardFillSystem != null)
        {
            FillBoardOnEnableAsync(); 
        }
    }

    /// <summary>
    /// 보드 채우기를 비동기로 수행
    /// </summary>
    private async void FillBoardOnEnableAsync()
    {
        if (_boardFillSystem != null)
        {
            // 초기 매치 제거 중 입력 차단
            _isProcessing = true;
            try
            {
                await _boardFillSystem.FillBoardFromStart();
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }

    private void OnDisable()
    {
        // 다른 게임으로 전환 시(비활성화 시) 현재 보드 상태를 큐에 저장
        if (ItemQueueManager.Instance != null && _boardFillSystem != null)
        {
            ReturnRemainingItemsToQueue();
        }
    }

    private void Update()
    {
        if (_isProcessing) return;
        
        if (_inputManager == null) return;

        HandleSwipeInput();
    }
    
    /// <summary>
    /// 스와이프 및 탭 입력 처리
    /// </summary>
    private void HandleSwipeInput()
    {
        // 드래그 시작 감지
        if (_inputManager.WasTappedThisFrame)
        {
            _swipeStartPos = _inputManager.PointerPosition;
            _swipeTile = GetTileAtPosition(_swipeStartPos);
            _isDragging = _swipeTile != null;
        }
        
        // 드래그 중
        if (_isDragging && _inputManager.IsPressing)
        {
            Vector2 currentPos = _inputManager.PointerPosition;
            Vector2 delta = currentPos - _swipeStartPos;
            
            // 스와이프 임계값을 넘으면 스와이프 처리
            if (delta.magnitude >= SwipeThreshold)
            {
                HandleSwipe(delta);
                _isDragging = false;
                _swipeTile = null;
            }
        }
        
        // 드래그 종료 (스와이프 없이 릴리즈된 경우 - 탭으로 처리)
        if (_isDragging && !_inputManager.IsPressing)
        {
            // 스와이프 임계값을 넘지 않았으면 탭으로 처리
            if (_swipeTile != null)
            {
                Select(_swipeTile);
            }
            _isDragging = false;
            _swipeTile = null;
        }
    }
    
    /// <summary>
    /// 스와이프 방향에 따라 타일 스왑
    /// </summary>
    private void HandleSwipe(Vector2 delta)
    {
        if (_swipeTile == null || _swipeTile.Item == null) return;
        
        // 스와이프 방향 결정 (상하좌우)
        Tile targetTile = null;
        
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // 좌우 스와이프
            if (delta.x > 0)
            {
                // 오른쪽 스와이프
                targetTile = GetNeighbourTile(_swipeTile, 1, 0);
            }
            else
            {
                // 왼쪽 스와이프
                targetTile = GetNeighbourTile(_swipeTile, -1, 0);
            }
        }
        else
        {
            // 상하 스와이프 (Unity UI는 Y가 위로 증가)
            if (delta.y > 0)
            {
                // 위쪽 스와이프 (배열에서는 y가 감소)
                targetTile = GetNeighbourTile(_swipeTile, 0, -1);
            }
            else
            {
                // 아래쪽 스와이프 (배열에서는 y가 증가)
                targetTile = GetNeighbourTile(_swipeTile, 0, 1);
            }
        }
        
        // 타겟 타일이 유효하면 스왑 실행
        if (targetTile != null && targetTile.Item != null && targetTile.button.interactable)
        {
            // TileSwapper를 통해 스왑 실행
            _tileSwapper.SwapTiles(_swipeTile, targetTile);
        }
    }
    
    /// <summary>
    /// 화면 위치에서 타일 가져오기
    /// </summary>
    private Tile GetTileAtPosition(Vector2 screenPosition)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            Tile tile = result.gameObject.GetComponent<Tile>();
            if (tile != null && tile.button.interactable && tile.Item != null)
            {
                return tile;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 인접 타일 가져오기
    /// </summary>
    private Tile GetNeighbourTile(Tile tile, int dx, int dy)
    {
        int newX = tile.x + dx;
        int newY = tile.y + dy;
        
        // 범위 체크
        if (newX < 0 || newX >= width || newY < 0 || newY >= height)
        {
            return null;
        }
        
        return Tiles[newX, newY];
    }

    /// <summary>
    /// 터치/클릭 위치에서 타일을 찾아 선택
    /// </summary>
    private void HandleTileTouch(Vector2 screenPosition)
    {

        // GraphicRaycaster를 사용하여 UI 요소 감지
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // 클릭된 UI 요소 중 Tile 컴포넌트를 가진 것 찾기
        foreach (RaycastResult result in results)
        {
            Tile tile = result.gameObject.GetComponent<Tile>();
            if (tile != null)
            {
                Select(tile);
                return; // 첫 번째 타일만 선택
            }
        }
    }

    /// <summary>
    /// BoardFillSystem 초기화
    /// </summary>
    private void InitializeBoardFillSystem()
    {
        _fillCursor = new BoardFillCursor();

        // ItemQueueManager 확인
        if (ItemQueueManager.Instance == null)
        {
            Debug.LogError("[Board] ItemQueueManager.Instance가 null입니다. ItemQueueManager를 씬에 추가해주세요.");
            return;
        }

        _boardFillSystem = new BoardFillSystem(
            rows,
            Tiles,
            _fillCursor,
            ItemQueueManager.Instance,
            _matchDetector,
            _popHandler
        );
    }

    /// <summary>
    /// 타일 선택 (외부에서 호출)
    /// </summary>
    public void Select(Tile tile)
    {
        if (_isProcessing) return;
        
        _tileSwapper.Select(tile);
    }
    
    /// <summary>
    /// 입력 처리 상태 설정 (TileSwapper에서 호출)
    /// </summary>
    public void SetProcessing(bool isProcessing)
    {
        _isProcessing = isProcessing;
    }

    // 새로운 아이템 추가 후 보드 Refill
    public void RefillBoard()
    {
        if (_boardFillSystem != null)
        {
            RefillBoardAsync(); // async 메서드 호출
        }
    }
    
    /// <summary>
    /// 보드 재채우기를 비동기로 수행
    /// </summary>
    private async void RefillBoardAsync()
    {
        if (_boardFillSystem != null)
        {
            // 초기 매치 제거 중 입력 차단
            _isProcessing = true;
            try
            {
                await _boardFillSystem.FillBoardFromStart();
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
    
    /// <summary>
    /// 테스트용: 보드 채우기를 비동기로 수행
    /// </summary>
    private async void TestFillBoardFromStartAsync()
    {
        if (_boardFillSystem != null)
        {
            // 초기 매치 제거 중 입력 차단
            _isProcessing = true;
            try
            {
                await _boardFillSystem.FillBoardFromStart();
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }

    //기존 큐를 초기화하고, 보드에 남은 아이템을 큐에 저장
    public void ReturnRemainingItemsToQueue()
    {
        if (ItemQueueManager.Instance == null)
        {
            Debug.LogWarning("[Board] ItemQueueManager.Instance null");
            return;
        }

        // 기존 큐 완전히 초기화
        ItemQueueManager.Instance.ClearQueue();
        Debug.Log("[Board] 기존 아이템 큐를 초기화");

        // 보드에 남은 아이템 수집 (왼쪽 아래부터 오른쪽으로, 위로 순서)
        // 왼쪽 아래(0,0) → 오른쪽 → 위 순서
        // Unity UI에서 Row[0]이 위쪽이므로 y를 역순으로 매핑
        var remainingItems = new List<Item>();
        
        int totalSlots = width * height;
        for (int index = 0; index < totalSlots; index++)
        {
            int x = index % width;
            int rowIndex = index / width;
            int y = height - 1 - rowIndex; 
            
            var tile = Tiles[x, y];
            if (tile != null && tile.Item != null && tile.button.interactable)
            {
                remainingItems.Add(tile.Item);
            }
        }

        // 보드에 남은 아이템을 큐에 저장 (기획 4.1)
        if (remainingItems.Count > 0)
        {
            ItemQueueManager.Instance.AddItems(remainingItems);
            Debug.Log($"[Board] 보드에 남은 {remainingItems.Count}개 아이템을 큐에 저장");
        }
        else
        {
            Debug.Log("[Board] 보드에 남은 아이템 없음");
        }

        // 다음 Match3 진입 시 항상 처음부터 채우기
        // (Board.Start()에서 FillBoardFromStart() 호출)
        
        Debug.Log($"[Board] 보드 아이템 반환 완료. 큐에 총 {ItemQueueManager.Instance.ItemCount}개 아이템이 있습니다.");
    }

#if UNITY_EDITOR
    /// <summary>
    /// 테스트: 랜덤 아이템을 큐에 추가하고 보드에 배치
    /// GameSceneManager.cs의 TestAddRandomItems()와 유사한 방식
    /// </summary>
    [ContextMenu("Test: Fill Board with Random Items")]
    void TestFillBoardWithRandomItems()
    {
        if (ItemQueueManager.Instance == null)
        {
            Debug.LogError("[Board] ItemQueueManager.Instance가 null");
            return;
        }

        if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0)
        {
            Debug.LogWarning("[Board] ItemDataBase에 아이템이 없습니다.");
            return;
        }

        if (_boardFillSystem == null)
        {
            Debug.LogError("[Board] BoardFillSystem이 초기화되지 않았습니다.");
            return;
        }

        // 기존 큐 초기화 (테스트를 위해)
        ItemQueueManager.Instance.ClearQueue();
        Debug.Log($"[Board 테스트] 기존 큐 초기화됨");

        // 랜덤 아이템 생성 및 큐에 추가
        var generatedItems = new List<Item>();
        for (int i = 0; i < testRandomItemCount; i++)
        {
            int randomIndex = Random.Range(0, ItemDataBase.Items.Length);
            Item randomItem = ItemDataBase.Items[randomIndex];
            ItemQueueManager.Instance.AddItem(randomItem);
            generatedItems.Add(randomItem);
        }

        Debug.Log($"[Board 테스트] {testRandomItemCount}개의 랜덤 아이템을 큐에 추가했습니다.");
        Debug.Log($"[Board 테스트] 생성된 아이템 리스트:");
        for (int i = 0; i < generatedItems.Count; i++)
        {
            Debug.Log($"  [{i}] {generatedItems[i].name}");
        }

        // 보드에 아이템 배치 (처음부터 채우기)
        TestFillBoardFromStartAsync(); // async 메서드 호출
        
        int totalSlots = width * height;
        int filledCount = 0;
        int emptyCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Tiles[x, y] != null && Tiles[x, y].Item != null)
                    filledCount++;
                else
                    emptyCount++;
            }
        }

        Debug.Log($"[Board 테스트] 보드 채우기 완료!");
        Debug.Log($"[Board 테스트] 전체 칸: {totalSlots}, 채워진 칸: {filledCount}, 빈 칸: {emptyCount}");
        Debug.Log($"[Board 테스트] 큐에 남은 아이템: {ItemQueueManager.Instance.ItemCount}개");
    }

    /// <summary>
    /// 테스트: 지정된 개수의 랜덤 아이템으로 보드 채우기 (커스텀 개수)
    /// </summary>
    /// <param name="itemCount">생성할 랜덤 아이템 개수</param>
    [ContextMenu("Test: Fill Board with 20 Random Items")]
    void TestFillBoardWith20Items() => TestFillBoardWithCustomCount(20);

    [ContextMenu("Test: Fill Board with 30 Random Items")]
    void TestFillBoardWith30Items() => TestFillBoardWithCustomCount(30);

    [ContextMenu("Test: Fill Board with 49 Random Items")]
    void TestFillBoardWith49Items() => TestFillBoardWithCustomCount(49);

    /// <summary>
    /// 테스트: 지정된 개수의 랜덤 아이템으로 보드 채우기
    /// </summary>
    void TestFillBoardWithCustomCount(int itemCount)
    {
        if (ItemQueueManager.Instance == null)
        {
            Debug.LogError("[Board] ItemQueueManager.Instance가 null입니다.");
            return;
        }

        if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0)
        {
            Debug.LogWarning("[Board] ItemDataBase에 아이템이 없습니다.");
            return;
        }

        if (_boardFillSystem == null)
        {
            Debug.LogError("[Board] BoardFillSystem이 초기화되지 않았습니다.");
            return;
        }

        // 기존 큐 초기화
        ItemQueueManager.Instance.ClearQueue();

        // 랜덤 아이템 생성 및 큐에 추가
        var generatedItems = new List<Item>();
        for (int i = 0; i < itemCount; i++)
        {
            int randomIndex = Random.Range(0, ItemDataBase.Items.Length);
            Item randomItem = ItemDataBase.Items[randomIndex];
            ItemQueueManager.Instance.AddItem(randomItem);
            generatedItems.Add(randomItem);
        }

        Debug.Log($"[Board 테스트] {itemCount}개의 랜덤 아이템으로 보드 채우기 시작");

        // 보드에 아이템 배치
        TestFillBoardFromStartAsync(); // async 메서드 호출
        
        // 주의: async 메서드이므로 실제 완료를 기다리지 않음 (테스트용)
        int totalSlots = width * height;
        int filledCount = 0;
        int emptyCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Tiles[x, y] != null && Tiles[x, y].Item != null)
                    filledCount++;
                else
                    emptyCount++;
            }
        }

        Debug.Log($"[Board 테스트] 보드 채우기 완료 - 전체: {totalSlots}, 채워진: {filledCount}, 빈칸: {emptyCount}, 큐 남음: {ItemQueueManager.Instance.ItemCount}");
    }

    /// <summary>
    /// 테스트: 보드 → 큐 저장 → 랜덤 아이템 추가 → 보드 재채우기
    /// 플러피 게임으로 이동했다 돌아오는 로직과 유사한 테스트
    /// </summary>
    [ContextMenu("Test: Board to Queue and Refill (Flappy Mode Test)")]
    void TestBoardToQueueAndRefill()
    {
        if (ItemQueueManager.Instance == null)
        {
            Debug.LogError("[Board] ItemQueueManager.Instance가 null입니다.");
            return;
        }

        if (ItemDataBase.Items == null || ItemDataBase.Items.Length == 0)
        {
            Debug.LogWarning("[Board] ItemDataBase에 아이템이 없습니다.");
            return;
        }

        if (_boardFillSystem == null)
        {
            Debug.LogError("[Board] BoardFillSystem이 초기화되지 않았습니다.");
            return;
        }

        Debug.Log("═══════════════════════════════════════");
        Debug.Log("[Board 테스트] 플러피 모드 전환 시뮬레이션 시작");
        Debug.Log("═══════════════════════════════════════");

        // 1. 현재 보드 상태 확인
        int initialFilledCount = 0;
        int initialEmptyCount = 0;
        var initialItems = new List<Item>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Tiles[x, y] != null && Tiles[x, y].Item != null && Tiles[x, y].button.interactable)
                {
                    initialFilledCount++;
                    initialItems.Add(Tiles[x, y].Item);
                }
                else
                {
                    initialEmptyCount++;
                }
            }
        }

        Debug.Log($"[1단계] 현재 보드 상태 - 채워진 칸: {initialFilledCount}, 빈 칸: {initialEmptyCount}");

        if (initialFilledCount == 0)
        {
            Debug.LogWarning("[Board 테스트] 보드에 아이템이 없습니다. 먼저 보드를 채워주세요.");
            return;
        }

        // 2. Match3 → Flappy 전환: 보드 아이템을 큐에 저장 (ReturnRemainingItemsToQueue 호출)
        Debug.Log($"[2단계] Match3 → Flappy 전환: 보드 아이템을 큐에 저장...");
        ReturnRemainingItemsToQueue();

        int savedItemCount = ItemQueueManager.Instance.ItemCount;
        Debug.Log($"[2단계 완료] 큐에 저장된 아이템: {savedItemCount}개");

        // 3. Flappy에서 랜덤 아이템 획득 (큐에 추가)
        Debug.Log($"[3단계] Flappy에서 {testAdditionalRandomItemCount}개의 랜덤 아이템 획득...");
        
        var newRandomItems = new List<Item>();
        for (int i = 0; i < testAdditionalRandomItemCount; i++)
        {
            int randomIndex = Random.Range(0, ItemDataBase.Items.Length);
            Item randomItem = ItemDataBase.Items[randomIndex];
            ItemQueueManager.Instance.AddItem(randomItem);
            newRandomItems.Add(randomItem);
        }

        Debug.Log($"[3단계 완료] {testAdditionalRandomItemCount}개의 랜덤 아이템을 큐에 추가했습니다.");
        Debug.Log($"[3단계 완료] 큐에 있는 총 아이템: {ItemQueueManager.Instance.ItemCount}개");

        // 4. Flappy → Match3 전환: 큐를 기반으로 보드 재채우기
        Debug.Log($"[4단계] Flappy → Match3 전환: 큐를 기반으로 보드 재채우기...");
        RefillBoard();

        // 5. 최종 보드 상태 확인
        int finalFilledCount = 0;
        int finalEmptyCount = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (Tiles[x, y] != null && Tiles[x, y].Item != null)
                    finalFilledCount++;
                else
                    finalEmptyCount++;
            }
        }

        int totalSlots = width * height;
        int queueRemaining = ItemQueueManager.Instance.ItemCount;

        Debug.Log("═══════════════════════════════════════");
        Debug.Log("[Board 테스트] 테스트 완료!");
        Debug.Log("═══════════════════════════════════════");
        Debug.Log($"[결과] 전체 칸: {totalSlots}");
        Debug.Log($"[결과] 채워진 칸: {finalFilledCount} (이전: {initialFilledCount})");
        Debug.Log($"[결과] 빈 칸: {finalEmptyCount} (이전: {initialEmptyCount})");
        Debug.Log($"[결과] 큐에 남은 아이템: {queueRemaining}개");
        Debug.Log($"[결과] 추가된 랜덤 아이템: {testAdditionalRandomItemCount}개");
        Debug.Log("═══════════════════════════════════════");
    }

    /// <summary>
    /// 테스트: 현재 큐 상태 출력
    /// </summary>
    [ContextMenu("Test: Debug Print Queue State")]
    void TestDebugPrintQueue()
    {
        if (ItemQueueManager.Instance != null)
        {
            ItemQueueManager.Instance.DebugPrintQueue();
        }
        else
        {
            Debug.LogWarning("[Board] ItemQueueManager.Instance가 null입니다.");
        }
    }
#endif
}

