using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

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

    private readonly List<Tile> _selection = new List<Tile>();

    private const float TweenDuration = 0.25f;

    private void Awake() => Instance = this;

    private void Start()
    {
        Screen.SetResolution(2340, 1080, true);

        // 무한 루프를 방지하기 위한 최대 시도 횟수
        const int maxTries = 1000;
        int currentTry = 0;

        do
        {
            CreateInitialBoard();

            // 세 개 이상의 연속된 버블이 없는지 확인
            if (!HasConsecutiveBubbles())
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


    public void CreateInitialBoard()
    {
        int boardWidth = rows.Max(row => row.tiles.Length);
        int boardHeight = rows.Length;
        Tiles = new Tile[boardWidth, boardHeight];

        // 테스트 모드인 경우 테스트 아이템 사용
        List<Item> collectedItems;
        if (useTestItems && testItems.Count > 0)
        {
            collectedItems = new List<Item>(testItems);
            Debug.Log($"[테스트 모드] 테스트 아이템 {collectedItems.Count}개 사용");
        }
        else
        {
            // 플래피버드에서 수집한 아이템 리스트 가져오기
            collectedItems = FlappyItemCollector.GetCollectedItems();
        }
        
        // 수집한 아이템이 없으면 기존 랜덤 방식 사용
        if (collectedItems.Count == 0)
        {
            CreateRandomBoard();
            return;
        }

        // 수집한 아이템을 순서대로 배치 (왼쪽 아래부터 오른쪽으로, 위로)
        int totalTiles = boardWidth * boardHeight;
        int itemIndex = 0;
        
        for (int index = 0; index < totalTiles; index++)
        {
            int x = index % boardWidth;
            int y = index / boardWidth;
            
            var tile = rows[y].tiles[x];
            tile.x = x;
            tile.y = y;
            
            // 수집한 아이템이 있으면 배치, 없으면 빈 칸
            if (itemIndex < collectedItems.Count)
            {
                tile.Item = collectedItems[itemIndex];
                tile.button.interactable = true; // 상호작용 가능
                itemIndex++;
            }
            else
            {
                tile.Item = null; // 빈 칸
                tile.button.interactable = false; // 상호작용 불가능
                tile.icon.gameObject.SetActive(false); // 아이콘 숨김
            }
            
            Tiles[x, y] = tile;
        }
        
        Debug.Log($"[보드 생성] {collectedItems.Count}개 아이템 배치 완료, 빈 칸 {totalTiles - collectedItems.Count}개");
    }

    private void CreateRandomBoard()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];
                tile.x = x;
                tile.y = y;
                tile.button.interactable = true; // 상호작용 가능

                do
                {
                    tile.Item = ItemDataBase.Items[Random.Range(0, ItemDataBase.Items.Length)];
                } while (IsConsecutiveTile(x, y, tile.Item));

                Tiles[x, y] = tile;
            }
        }
    }

    public bool HasConsecutiveBubbles()
    {
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width - 2; x++)
            {
                if (AreConsecutiveTiles(Tiles[x, y], Tiles[x + 1, y], Tiles[x + 2, y]))
                    return true;
            }
        }

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height - 2; y++)
            {
                if (AreConsecutiveTiles(Tiles[x, y], Tiles[x, y + 1], Tiles[x, y + 2]))
                    return true;
            }
        }

        return false;
    }

    private bool IsConsecutiveTile(int x, int y, Item item)
    {
        // 현재 타일 주변의 타일들이 같은 아이템인지 확인
        return (x > 1 && Tiles[x - 1, y].Item == item && Tiles[x - 2, y].Item == item) ||
               (y > 1 && Tiles[x, y - 1].Item == item && Tiles[x, y - 2].Item == item);
    }



    public async void Select(Tile tile)
    {
        // 빈 타일은 선택 불가능
        if (tile == null || tile.Item == null)
        {
            return;
        }
        
        if (!_selection.Contains(tile))
        {
            if (_selection.Count > 0)
            {
                if (Array.IndexOf(_selection[0].Neighbours, tile) != -1)
                {
                    _selection.Add(tile);
                }
            }
            else
            {
                _selection.Add(tile);
            }
        }



        if (_selection.Count < 2) return;

        Debug.Log($"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            while (CanPop()) await Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);
        }

        _selection.Clear();
    }
    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
            .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        await sequence.Play().AsyncWaitForCompletion();

        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tile1Item = tile1.Item;

        tile1.Item = tile2.Item;
        tile2.Item = tile1Item;
    }

    private bool CanPop()
    {
        // 각 행을 확인
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width - 2; x++)
            {
                if (AreConsecutiveTiles(Tiles[x, y], Tiles[x + 1, y], Tiles[x + 2, y]))
                    return true;
            }
        }

        // 각 열을 확인
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height - 2; y++)
            {
                if (AreConsecutiveTiles(Tiles[x, y], Tiles[x, y + 1], Tiles[x, y + 2]))
                    return true;
            }
        }

        return false;
    }

    private bool AreConsecutiveTiles(Tile tile1, Tile tile2, Tile tile3)
    {
        // 빈 타일이 있으면 false 반환
        if (tile1 == null || tile2 == null || tile3 == null ||
            tile1.Item == null || tile2.Item == null || tile3.Item == null)
        {
            return false;
        }
        
        // 타일이 서로 같은 종류이면서 연속되어 있는지 확인
        return (tile1.Item == tile2.Item && tile2.Item == tile3.Item);
    }


    private async Task<bool> Pop()
    {
        // 일렬로 연속된 같은 타일이 3개 이상 있는 경우에만 처리
        if (CanPop())
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var tile = Tiles[x, y];
                    
                    // 빈 타일은 건너뛰기
                    if (tile == null || tile.Item == null)
                        continue;

                    // 타일 주변의 같은 타일 수를 확인
                    int horizontalCount = CountSameTilesInDirection(tile, Vector2.right);
                    int verticalCount = CountSameTilesInDirection(tile, Vector2.up);

                    if (horizontalCount >= 2 && x + horizontalCount - 1 < width)
                    {
                        var connectedTiles = new List<Tile>();

                        for (int i = 0; i < horizontalCount; i++)
                        {
                            connectedTiles.Add(Tiles[x + i, y]);
                        }

                        await PopConnectedTiles(connectedTiles);
                    }

                    if (verticalCount >= 2 && y + verticalCount - 1 < height)
                    {
                        var connectedTiles = new List<Tile>();

                        for (int i = 0; i < verticalCount; i++)
                        {
                            connectedTiles.Add(Tiles[x, y + i]);
                        }

                        await PopConnectedTiles(connectedTiles);
                    }
                }
            }

            // 연속된 조건이 하나라도 처리되었다면 true 반환
            return true;
        }

        // 연속된 조건이 하나도 없으면 false 반환
        return false;
    }



    private async Task PopConnectedTiles(List<Tile> connectedTiles)
    {
        if (connectedTiles.Count >= 3)
        {
            var deflateSequence = DOTween.Sequence();
            var colors = new Dictionary<Item, int>(); // 각 색깔의 타일 개수를 저장할 딕셔너리

            foreach (var connectedTile in connectedTiles)
            {
                // 빈 타일은 건너뛰기
                if (connectedTile == null || connectedTile.Item == null || !connectedTile.button.interactable)
                    continue;
                
                deflateSequence.Join(connectedTile.icon.transform.
                    DOScale(Vector3.zero, TweenDuration));

                // 색깔 별 타일 개수 세기
                if (!colors.ContainsKey(connectedTile.Item))
                {
                    colors[connectedTile.Item] = 1;
                }
                else
                {
                    colors[connectedTile.Item]++;
                }
            }

            audioSource.PlayOneShot(collectSound);

            // 각 색깔 별로 개별적으로 점수 계산
            foreach (var colorCount in colors)
            {
                //Score.Instance.AddScore(colorCount.Key, colorCount.Key.value * colorCount.Value);
            }

            await deflateSequence.Play().AsyncWaitForCompletion();

            var inflateSequence = DOTween.Sequence();

            foreach (var connectedTile in connectedTiles)
            {
                // 빈 타일이면 새 아이템 생성하지 않음
                if (connectedTile == null || connectedTile.button.interactable == false)
                {
                    continue;
                }
                
                connectedTile.Item = ItemDataBase.
                    Items[Random.Range(0, ItemDataBase.Items.Length)];

                inflateSequence.Join(connectedTile.icon.transform.
                    DOScale(Vector3.one, TweenDuration));
            }

            await inflateSequence.Play().AsyncWaitForCompletion();
        }
    }


    private int CountSameTilesInDirection(Tile startTile, Vector2 direction)
    {
        int count = 1; // 자기 자신 포함

        int x = startTile.x;
        int y = startTile.y;

        while (true)
        {
            x += (int)direction.x;
            y += (int)direction.y;

            if (x < 0 || x >= width || y < 0 || y >= height)
                break;

            var nextTile = Tiles[x, y];
            // 빈 타일이면 중단
            if (nextTile == null || nextTile.Item == null)
                break;

            if (nextTile.Item == startTile.Item)
                count++;
            else
                break;
        }

        return count;
    }


}