using UnityEngine;

namespace FallingDodge.Configs
{
    [CreateAssetMenu(fileName = "FallingDodgeConfig", menuName = "Scriptable Objects/FallingDodgeConfig")]
    public class FallingDodgeConfig : ScriptableObject
    {
        [Header("Spawn Area")]
        [Tooltip("떨어지는 오브젝트가 생성될 수 있는 가장 왼쪽 X 위치입니다. 값이 작을수록 더 왼쪽까지 생성됩니다.")]
        public float SpawnMinX = -2.8f;
        [Tooltip("떨어지는 오브젝트가 생성될 수 있는 가장 오른쪽 X 위치입니다. 값이 클수록 더 오른쪽까지 생성됩니다.")]
        public float SpawnMaxX = 2.8f;
        [Tooltip("떨어지는 오브젝트가 처음 생성되는 Y 위치입니다. 값이 클수록 화면 위쪽에서 더 일찍 보입니다.")]
        public float SpawnY = 6.2f;

        [Header("Fall Speed")]
        [Tooltip("아이템의 최소 낙하 속도입니다. 값이 클수록 가장 느린 아이템도 더 빠르게 떨어집니다.")]
        [Min(0.0f)] public float ItemFallSpeedMin = 3.2f;
        [Tooltip("아이템의 최대 낙하 속도입니다. 값이 클수록 빠른 아이템이 더 빠르게 떨어집니다.")]
        [Min(0.0f)] public float ItemFallSpeedMax = 4.2f;
        [Tooltip("위험물의 최소 낙하 속도입니다. 값이 클수록 가장 느린 위험물도 더 빠르게 떨어집니다.")]
        [Min(0.0f)] public float PoopFallSpeedMin = 4.0f;
        [Tooltip("위험물의 최대 낙하 속도입니다. 값이 클수록 빠른 위험물이 더 빠르게 떨어집니다.")]
        [Min(0.0f)] public float PoopFallSpeedMax = 5.2f;

        [Header("Difficulty Scaling")]
        [Tooltip("초당 낙하 속도 배율이 증가하는 양입니다. 값이 클수록 게임 시간이 지날 때 낙하물과 플레이어가 더 빠르게 가속됩니다.")]
        [Min(0.0f)] public float FallSpeedAcceleration = 0.0125f;
        [Tooltip("낙하 속도와 플레이어 이동 속도에 적용되는 최대 배율입니다. 1이면 가속되지 않고, 값이 클수록 후반 속도가 더 빨라집니다.")]
        [Min(1.0f)] public float MaxFallSpeedMultiplier = 1.8f;

        [Header("Spawn Timing")]
        [Tooltip("스폰이 시작되기 전 대기 시간입니다. 값이 클수록 게임 시작 후 첫 오브젝트가 늦게 나옵니다.")]
        [Min(0.0f)] public float InitialSpawnDelay = 1.5f;
        [Tooltip("초반 스폰 간격입니다. 값이 작을수록 초반부터 오브젝트가 더 자주 생성됩니다.")]
        [Min(0.01f)] public float BaseSpawnInterval = 0.7f;
        [Tooltip("난이도가 오른 뒤 도달하는 최소 스폰 간격입니다. 값이 작을수록 후반에 더 촘촘하게 생성됩니다.")]
        [Min(0.01f)] public float MinimumSpawnInterval = 0.22f;

        [Header("Hazard Scaling")]
        [Tooltip("게임 시작 시 위험물이 나올 확률입니다. 0은 위험물 없음, 1은 전부 위험물입니다.")]
        [Range(0.0f, 1.0f)] public float PoopChanceAtStart = 0.12f;
        [Tooltip("난이도가 오른 뒤 위험물이 나올 최대 확률입니다. 값이 클수록 후반에 위험물이 더 자주 나옵니다.")]
        [Range(0.0f, 1.0f)] public float PoopChanceAtMax = 0.65f;
        [Tooltip("스폰 간격과 위험물 확률이 최대 난이도에 도달하는 데 걸리는 시간입니다. 값이 작을수록 난이도가 빠르게 올라갑니다.")]
        [Min(0.01f)] public float PoopRampDuration = 70f;

        [Header("Wave Settings")]
        [Tooltip("미리 만들어둘 낙하 오브젝트 풀 크기입니다. 한 번에 많이 생성된다면 값을 키워 런타임 생성 비용을 줄일 수 있습니다.")]
        [Min(1)] public int InitialPoolSize = 20;
        [Tooltip("한 번의 스폰 웨이브에서 생성되는 최소 오브젝트 개수입니다. 값이 클수록 항상 더 많은 오브젝트가 떨어집니다.")]
        [Min(1)] public int MinSpawnCountPerWave = 1;
        [Tooltip("한 번의 스폰 웨이브에서 생성되는 최대 오브젝트 개수입니다. 값이 클수록 한 번에 더 많은 오브젝트가 떨어질 수 있습니다.")]
        [Min(1)] public int MaxSpawnCountPerWave = 2;

        [Header("Spawn Ranges")]
        [Tooltip("스폰 영역을 나눌 가로 구간 개수입니다. 웨이브 생성 시 여러 구간에 분산해 겹침을 줄이는 데 사용됩니다.")]
        [Min(1)] public int SpawnRangeCount = 5;
        [Tooltip("각 스폰 구간 양끝에서 비워둘 여백입니다. 값이 클수록 오브젝트가 구간 중앙 쪽에 더 모여 생성됩니다.")]
        [Min(0.0f)] public float SpawnRangePadding = 0.15f;
    }
}
