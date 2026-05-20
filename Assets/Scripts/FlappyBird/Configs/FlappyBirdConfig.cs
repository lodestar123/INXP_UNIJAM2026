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
        [Tooltip("버튼을 누르고 있을 때 가해지는 힘입니다. 예: 25면 버튼을 누르고 있는 동안 매 프레임마다 25의 힘이 가해집니다.")]
        [Min(0.0f)] public float HoldForce = 25.0f; // 버튼을 누르고 있을 때 가해지는 힘
        [Tooltip("버튼을 처음 눌렀을 때 가해지는 순간적인 힘입니다. 예: 4면 버튼을 처음 눌렀을 때 4의 힘이 순간적으로 가해집니다.")]
        [Min(0.0f)] public float PressImpulse = 4.0f;
        [Tooltip("버튼에서 손을 뗄 때 현재 상승 속도의 몇 배를 추가로 가할지 결정하는 계수입니다. 예: 0.5면 현재 상승 속도의 절반만큼 추가로 가합니다.")]
        [Range(0.0f, 1.0f)] public float ReleaseUpVelocityMultiplier = 0.35f;
        [Tooltip("버튼에서 손을 뗄 때 현재 하강 속도의 몇 배를 추가로 가할지 결정하는 계수입니다. 예: 0.5면 현재 하강 속도의 절반만큼 추가로 가합니다.")]
        [Min(0.0f)] public float ReleaseDownImpulse = 0.0f;
        [Tooltip("버튼에서 손을 뗄 때 상승 속도가 최대 상승 속도를 초과하지 않도록 하는 계수입니다. 예: 0.5면 현재 상승 속도의 절반만큼 추가로 가하되, 최대 상승 속도를 초과하지 않도록 합니다.")]
        [Min(0.0f)] public float MaxUpVelocity = 6.0f; // 최대 상승 속도
        [Tooltip("버튼에서 손을 뗄 때 하강 속도가 최대 하강 속도를 초과하지 않도록 하는 계수입니다. 예: 0.5면 현재 하강 속도의 절반만큼 추가로 가하되, 최대 하강 속도를 초과하지 않도록 합니다.")]
        [Min(0.0f)] public float MaxDownVelocity = 10.0f; // 최대 하강 속도

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
        [Tooltip("파이프의 기준 Y축 크기입니다. 아이템 배치 간격 계산에 사용됩니다.")]
        public float PipeSize = 8.0f;

        [Tooltip("갈림길 중앙 파이프의 기준 Y축 크기입니다.")]
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
