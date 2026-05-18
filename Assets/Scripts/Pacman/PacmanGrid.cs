using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pacman
{
    /// <summary>
    /// Pacman Tilemap을 셀 단위 길찾기 정보로 읽음.
    /// 벽 Tilemap에 타일이 있으면 벽, 없으면 길로 간주함.
    /// </summary>
    public class PacmanGrid : MonoBehaviour
    {
        // 원작 팩맨 방향 결정에 가깝게 Up, Left, Down, Right 순서로 검사함.
        public static readonly Vector2Int[] DirectionOrder =
        {
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down,
            Vector2Int.right,
        };

        [SerializeField] private Grid grid;
        // 벽으로 사용하는 Tilemap. 타일이 있는 셀은 이동 불가.
        [SerializeField] private Tilemap wallTilemap;
        [SerializeField] private bool autoFindReferences = true;

        public Grid Grid => grid;
        public Tilemap WallTilemap => wallTilemap;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            if (autoFindReferences)
            {
                ResolveReferences();
            }
        }

        public void ResolveReferences()
        {
            if (!autoFindReferences)
            {
                return;
            }

            if (grid == null)
            {
                grid = GetComponentInChildren<Grid>();
            }

            if (wallTilemap == null)
            {
                // 현재 Stage3 구조 기준으로 Pacman 하위 Tilemap 자동 탐색함.
                wallTilemap = GetComponentInChildren<Tilemap>();
            }
        }

        /// <summary>
        /// 월드 좌표를 Tilemap 셀 좌표로 변환함.
        /// </summary>
        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            ResolveReferences();
            return grid != null ? grid.WorldToCell(worldPosition) : Vector3Int.RoundToInt(worldPosition);
        }

        public Vector3 CellToWorldCenter(Vector3Int cell)
        {
            ResolveReferences();
            return grid != null ? grid.GetCellCenterWorld(cell) : cell;
        }

        public bool IsWall(Vector3Int cell)
        {
            ResolveReferences();
            return wallTilemap != null && wallTilemap.HasTile(cell);
        }

        /// <summary>
        /// 길찾기에서 사용 가능한 셀인지 확인함.
        /// </summary>
        public bool IsWalkable(Vector3Int cell)
        {
            return !IsWall(cell);
        }

        /// <summary>
        /// 현재 셀에서 이동 가능한 방향만 results에 담음.
        /// </summary>
        public int GetWalkableDirections(Vector3Int cell, List<Vector2Int> results)
        {
            results.Clear();

            for (int i = 0; i < DirectionOrder.Length; i++)
            {
                Vector2Int direction = DirectionOrder[i];
                if (IsWalkable(cell + ToCellOffset(direction)))
                {
                    results.Add(direction);
                }
            }

            return results.Count;
        }

        public static Vector3Int ToCellOffset(Vector2Int direction)
        {
            return new Vector3Int(direction.x, direction.y, 0);
        }

        /// <summary>
        /// 플레이어 Vector2 이동 방향을 Tilemap 셀 방향으로 변환함.
        /// </summary>
        public static Vector2Int ToGridDirection(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                return Vector2Int.zero;
            }

            return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? new Vector2Int(direction.x > 0f ? 1 : -1, 0)
                : new Vector2Int(0, direction.y > 0f ? 1 : -1);
        }
    }
}
