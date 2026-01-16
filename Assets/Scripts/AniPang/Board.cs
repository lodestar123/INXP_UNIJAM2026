using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    }
    /// <summary>
    /// 타일 선택 (외부에서 호출)
    /// </summary>
    public void Select(Tile tile)
    {
        _tileSwapper.Select(tile);
    }
}

