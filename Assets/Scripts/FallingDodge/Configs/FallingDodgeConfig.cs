using UnityEngine;

namespace FallingDodge.Configs
{
    [CreateAssetMenu(fileName = "FallingDodgeConfig", menuName = "Scriptable Objects/FallingDodgeConfig")]
    public class FallingDodgeConfig : ScriptableObject
    {
        [Header("Spawn Area")]
        public float SpawnMinX = -2.8f;
        public float SpawnMaxX = 2.8f;
        public float SpawnY = 6.2f;

        [Header("Fall Speed")]
        [Min(0.0f)] public float ItemFallSpeedMin = 3.2f;
        [Min(0.0f)] public float ItemFallSpeedMax = 4.2f;
        [Min(0.0f)] public float PoopFallSpeedMin = 4.0f;
        [Min(0.0f)] public float PoopFallSpeedMax = 5.2f;

        [Header("Difficulty Scaling")]
        [Min(0.0f)] public float FallSpeedAcceleration = 0.0125f;
        [Min(1.0f)] public float MaxFallSpeedMultiplier = 1.8f;

        [Header("Spawn Timing")]
        [Min(0.0f)] public float InitialSpawnDelay = 1.5f;
        [Min(0.01f)] public float BaseSpawnInterval = 0.7f;
        [Min(0.01f)] public float MinimumSpawnInterval = 0.22f;

        [Header("Hazard Scaling")]
        [Range(0.0f, 1.0f)] public float PoopChanceAtStart = 0.12f;
        [Range(0.0f, 1.0f)] public float PoopChanceAtMax = 0.65f;
        [Min(0.01f)] public float PoopRampDuration = 70f;

        [Header("Wave Settings")]
        [Min(1)] public int InitialPoolSize = 20;
        [Min(1)] public int MinSpawnCountPerWave = 1;
        [Min(1)] public int MaxSpawnCountPerWave = 2;

        [Header("Spawn Ranges")]
        [Min(1)] public int SpawnRangeCount = 5;
        [Min(0.0f)] public float SpawnRangePadding = 0.15f;
    }
}
