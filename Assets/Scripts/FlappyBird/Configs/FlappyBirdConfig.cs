using UnityEngine;
using UnityEngine.Serialization;

namespace FlappyBird.Configs
{
    /// <summary>
    /// 플래피버드 플레이와 장애물 생성에 쓰는 설정값 모음입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "FlappyBirdConfig", menuName = "Scriptable Objects/FlappyBirdConfig")]
    public class FlappyBirdConfig : ScriptableObject
    {
        [Header("플레이어 설정")]
        [Tooltip("버튼을 누르고 있는 동안 플레이어가 위로 올라가는 빠르기입니다. 값이 클수록 누르고 있을 때 더 빠르게 위로 올라갑니다.")]
        [Min(0.0f)] public float HoldForce = 25.0f; // 버튼을 누르고 있을 때 가해지는 힘
        [Tooltip("버튼을 처음 눌렀을 때 플레이어가 한 번에 튀어 오르는 세기입니다. 값이 클수록 입력 직후 더 빠르게 위로 치솟습니다.")]
        [Min(0.0f)] public float PressImpulse = 4.0f;
        [Tooltip("버튼에서 손을 뗄 때 남아 있는 상승 속도를 얼마나 유지할지 정합니다. 0에 가까울수록 바로 꺾이고, 1에 가까울수록 더 오래 위로 올라갑니다.")]
        [Range(0.0f, 1.0f)] public float ReleaseUpVelocityMultiplier = 0.35f;
        [Tooltip("버튼에서 손을 뗄 때 아래로 떨어지기 시작하는 세기입니다. 값이 클수록 손을 뗀 직후 더 빠르게 아래로 내려갑니다.")]
        [Min(0.0f)] public float ReleaseDownImpulse = 0.0f;
        [Tooltip("플레이어가 위로 올라갈 수 있는 최대 속도입니다. 값이 클수록 아무리 빠르게 올라가도 더 높은 속도까지 허용됩니다.")]
        [Min(0.0f)] public float MaxUpVelocity = 6.0f;
        [Tooltip("플레이어가 아래로 떨어질 수 있는 최대 속도입니다. 값이 클수록 더 빠르게 떨어질 수 있습니다.")]
        [Min(0.0f)] public float MaxDownVelocity = 10.0f;

        [Header("파이프 기본 설정")]
        [Min(0.1f)] public float PipeSpawnInterval = 1.2f;
        [Min(0.0f)] public float PipeMoveSpeed = 3.5f;

        [Header("가속 설정")]
        [Min(0.0f)] public float Acceleration = 0.1f;      // 초당 스크롤 속도 증가량
        [Min(0.0f)] public float MaxMoveSpeed = 10.0f;     // 스크롤 최대 속도

        public float PipeSpawnX = 18.0f;
        public float PipeMinY = -2.0f;
        public float PipeMaxY = 2.0f;

        [Header("파이프 프리팹 설정")]
        public GameObject TopPipePrefab;
        public GameObject BottomPipePrefab;
        public GameObject BranchPipePrefab;

        [Header("파이프 크기 설정 (간격 계산용)")]
        [Tooltip("파이프 하나가 세로로 차지하는 기준 크기입니다. 아이템 경로가 파이프와 겹치지 않도록 간격을 계산할 때 사용합니다.")]
        public float PipeSize = 8.0f;

        [Tooltip("갈림길 가운데 파이프가 세로로 차지하는 기준 크기입니다. 값이 클수록 가운데 파이프를 더 큰 장애물로 계산합니다.")]
        public float InnerPipeSize = 2.0f;

        [Header("파이프 패턴 설정")]
        [Min(0.1f)] public float GapHeight = 3.0f;
        [Min(0.0f)] public float PipeHeightVariance = 1.5f;
        [Range(0.0f, 1.0f)] public float DoublePipeChance = 0.3f;
        [Min(0.0f)] public float DoublePipeVerticalSpacing = 3.0f;

        [Header("아이템 경로 설정")]
        public GameObject ItemPrefab;
        [Min(0.1f)] public float ItemPathSpacing = 0.8f;
    }
}
