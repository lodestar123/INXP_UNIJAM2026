using UnityEngine;

namespace Pacman
{
    [CreateAssetMenu(fileName = "PacmanConfig", menuName = "Scriptable Objects/PacmanConfig")]
    public class PacmanConfig : ScriptableObject
    {
        [Header("Player Input")]
        [Min(0f)] public float playerSwipeThreshold = 60f;
        public bool allowKeyboardInEditor = true;

        [Header("Player Movement")]
        [Min(0f)] public float playerMoveSpeed = 4f;
        public Vector2 playerInitialDirection = Vector2.zero;
        public Vector2 playerObstacleBoxSize = Vector2.one * 0.75f;
        [Min(0f)] public float playerObstacleDistance = 1.5f;

        [Header("Ghost Mode")]
        public bool cycleScatterAndChase = true;
        public bool startInScatter = false;
        [Min(0.1f)] public float scatterDuration = 3f;
        [Min(0.1f)] public float chaseDuration = 24f;
        public bool useClassicScatterCorners = true;

        [Header("Ghost Movement")]
        [Min(0f)] public float ghostMoveSpeed = 3.5f;
        [Min(0f)] public float ghostCenterReachDistance = 0.015f;
        public bool centerGhostBetweenTwoCellCorridors = true;
        [Min(1)] public int ghostWalkableLookAheadCells = 2;
        [Min(0)] public int ghostRecentCellAvoidanceCount = 8;

        [Header("Ghost Catch")]
        public bool catchPlayerOnTouch = true;

        [Header("Items")]
        [Min(0)] public int itemCount = 49;
        [Min(0f)] public float itemScale = 0.3f;
        public bool spawnItemsOnEnable = true;
        public bool collectSpawnPointsFromRoot = true;
        public bool createDefaultSpawnPointsIfMissing = true;
        [Min(1)] public int sameItemSpawnGroupSize = 3;
    }
}
