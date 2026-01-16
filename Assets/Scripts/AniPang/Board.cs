using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Core.Input;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip collectSound;

    [SerializeField] private AudioSource audioSource;

    public Row[] rows;

    [Header("테스트 설정")]
    [Tooltip("테스트 모드 활성화 시, 이 리스트의 아이템으로 보드를 생성합니다.")]
    [SerializeField] private bool useTestItems = false;
    [SerializeField] private List<Item> testItems = new List<Item>();

    public Tile[,] Tiles { get; private set; }

    public int width => Tiles.GetLength(0);
    public int height => Tiles.GetLength(1);

    // 핸들러 클래스들
    private MatchDetector _matchDetector;
    private TileSwapper _tileSwapper;
    private PopHandler _popHandler;
    private GravityHandler _gravityHandler;
    private BoardInitializer _boardInitializer;

    // UnifiedInputManager 참조
    private IUnifiedInput _inputManager;

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
        _popHandler = new PopHandler(Tiles, _matchDetector, _gravityHandler, audioSource, collectSound);
        _tileSwapper = new TileSwapper(Tiles, _matchDetector, _popHandler);
        _boardInitializer = new BoardInitializer(rows, Tiles, _matchDetector, useTestItems, testItems);

        // 무한 루프를 방지하기 위한 최대 시도 횟수
        const int maxTries = 1000;
        int currentTry = 0;

        do
        {
            _boardInitializer.CreateInitialBoard();

            // 세 개 이상의 연속된 버블이 없는지 확인
            if (!_matchDetector.HasConsecutiveBubbles())
            {
                // 조건을 만족할 때까지 다시 시도
                currentTry++;
            }
            else
            {
                break;
            }
        } while (currentTry < maxTries);

        // UnifiedInputManager 초기화
        _inputManager = UnifiedInputManager.Instance;
    }

    private void Update()
    {
        // UnifiedInputManager를 사용하여 터치/클릭 감지
        if (_inputManager == null) return;

        if (_inputManager.WasTappedThisFrame)
        {
            HandleTileTouch(_inputManager.PointerPosition);
        }
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
    /// 타일 선택 (외부에서 호출)
    /// </summary>
    public void Select(Tile tile)
    {
        _tileSwapper.Select(tile);
    }
}

