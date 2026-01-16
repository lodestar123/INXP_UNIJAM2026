//using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    // 예지 추가한 변수(2개)
    public GameObject StartPanel_hj; // start 패널
    public AudioSource S_start_hj;  // start 소리
    public AudioSource S_game_hj;
    // 예지 추가 끝
    public static Board Instance { get; private set; }

    [SerializeField] private AudioClip collectSound;

    [SerializeField] private AudioSource audioSource;

    public Row[] rows;

    public Tile[,] Tiles { get; private set; }

    public int width => Tiles.GetLength(0);
    public int height => Tiles.GetLength(1);

    private readonly List<Tile> _selection = new List<Tile>();

    private const float TweenDuration = 0.25f;

    private void Awake() => Instance = this;

    private void Start()
    {
        Screen.SetResolution(2340, 1080, true);
        // 여기부터 예지 추가, 시작할 때 3초동안 게임 설명 화면 뜸. 
        // Show the StartPanel
        S_start_hj.Play();
        StartPanel_hj.SetActive(true);

        // Use a coroutine to hide the StartPanel after 3 seconds
        StartCoroutine(HideStartPanel_hj());

        // 예지 추가 끝

        // 무한 루프를 방지하기 위한 최대 시도 횟수
        const int maxTries = 1000;
        int currentTry = 0;

        do
        {
            // 초기 보드 생성
            CreateInitialBoard();

            // 세 개 이상의 연속된 버블이 없는지 확인
            if (!HasConsecutiveBubbles())
            {
                // 조건을 만족할 때까지 다시 시도
                currentTry++;
            }
            else
            {
                // 조건을 만족하면 루프 종료
                break;
            }
        } while (currentTry < maxTries);

    }

    // 예지 추가 시작
    // 3초 뒤 문구 없어짐
    IEnumerator HideStartPanel_hj()
    {
        yield return new WaitForSeconds(3f);

        // Hide the StartPanel after 3 seconds
        StartPanel_hj.SetActive(false);

        S_game_hj.Play();
    }
    // 예지 추가 끝


    public void CreateInitialBoard()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                // 아래 랜덤 생성 코드에서 특정 타일이 연속되는지 확인
                do
                {
                    //tile.Item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];
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



        _selection.Clear();


    }
    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        //var sequence = DOTween.Sequence();

        //sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
        //    .Join(icon2Transform.DOMove(icon1Transform.position, TweenDuration));

        //await sequence.Play().AsyncWaitForCompletion();

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
           // var deflateSequence = DOTween.Sequence();
            var colors = new Dictionary<Item, int>(); // 각 색깔의 타일 개수를 저장할 딕셔너리

            foreach (var connectedTile in connectedTiles)
            {
           //     deflateSequence.Join(connectedTile.icon.transform.
           //         DOScale(Vector3.zero, TweenDuration));

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
            // foreach (var colorCount in colors)
            // {
            //     Score.Instance.AddScore(colorCount.Key, colorCount.Key.value * colorCount.Value);
            // }

            // await deflateSequence.Play().AsyncWaitForCompletion();

            // var inflateSequence = DOTween.Sequence();

            // foreach (var connectedTile in connectedTiles)
            // {
            //     connectedTile.Item = ItemDatabase.
            //         Items[Random.Range(0, ItemDatabase.Items.Length)];

            //     inflateSequence.Join(connectedTile.icon.transform.
            //         DOScale(Vector3.one, TweenDuration));
            // }

            // await inflateSequence.Play().AsyncWaitForCompletion();
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

            if (Tiles[x, y].Item == startTile.Item)
                count++;
            else
                break;
        }

        return count;
    }


}