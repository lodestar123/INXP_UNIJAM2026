using UnityEngine;
using UnityEngine.Serialization;

namespace FlappyBird.Configs
{
    [CreateAssetMenu(fileName = "FlappyBirdConfig", menuName = "Scriptable Objects/FlappyBirdConfig")]
    public class FlappyBirdConfig : ScriptableObject
    {
        [Header("플레이어 설정")]
        [Min(0.0f)] public float HoldForce = 25.0f;
        [Min(0.0f)] public float MaxUpVelocity = 6.0f;
        [Min(0.0f)] public float MaxDownVelocity = 10.0f;

        [Header("파이프 기본 설정")]
        [Min(0.1f)] public float PipeSpawnInterval = 1.2f;
        [Min(0.0f)] public float PipeMoveSpeed = 3.5f;
        
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