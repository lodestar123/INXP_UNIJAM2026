using UnityEngine;

namespace FlappyBird.Configs
{
    /// <summary>
    /// 플래피 버드 게임의 밸런스와 설정을 담는 데이터 클래스입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "FlappyBirdConfig", menuName = "Scriptable Objects/FlappyBirdConfig")]
    public class FlappyBirdConfig : ScriptableObject
    {
        [Header("플레이어 설정")]
        [Tooltip("터치 시 위로 솟구치는 힘의 크기입니다.")]
        [Min(0.0f)] public float HoldForce = 25.0f;
        
        [Tooltip("위쪽으로 이동할 때의 최대 속도 제한입니다.")]
        [Min(0.0f)] public float MaxUpVelocity = 6.0f;
        
        [Tooltip("아래쪽으로 떨어질 때의 최대 속도 제한입니다.")]
        [Min(0.0f)] public float MaxDownVelocity = 10.0f;

        [Header("파이프 기본 설정")]
        [Tooltip("파이프가 생성되는 시간 간격(초)입니다.")]
        [Min(0.1f)] public float PipeSpawnInterval = 1.2f;
        
        [Tooltip("파이프가 왼쪽으로 이동하는 속도입니다.")]
        [Min(0.0f)] public float PipeMoveSpeed = 3.5f;
        
        [Tooltip("파이프가 생성되는 X 좌표 위치입니다.")]
        public float PipeSpawnX = 10.0f;
        
        [Tooltip("파이프가 생성될 수 있는 최소 Y 좌표입니다.")]
        public float PipeMinY = -2.0f;
        
        [Tooltip("파이프가 생성될 수 있는 최대 Y 좌표입니다.")]
        public float PipeMaxY = 2.0f;

        [Tooltip("위 아래 파이프의 크기입니다.")] 
        public float PipeSize = 8.0f;
        
        [Tooltip("갈래길 파이프의 크기입니다.")] 
        public float InnerPipeSize = 2.0f;


        [Header("파이프 생성 로직")]
        [Tooltip("위 파이프와 아래 파이프 사이의 수직 간격입니다.")]
        [Min(0.1f)] public float GapHeight = 3.0f; 
        
        [Tooltip("이전 파이프 위치 대비 Y 좌표가 변할 수 있는 최대 범위입니다.")]
        [Min(0.0f)] public float PipeHeightVariance = 1.5f;
        
        [Tooltip("갈림길(3개 파이프) 패턴이 등장할 확률입니다 (0.0 ~ 1.0).")]
        [Range(0.0f, 1.0f)] public float DoublePipeChance = 0.3f;
        
        [Tooltip("갈림길 패턴에서 위/아래 통로 사이의 간격입니다.")]
        [Min(0.0f)] public float DoublePipeVerticalSpacing = 3.0f;

        [Header("아이템 생성 설정")]
        [Tooltip("생성될 아이템의 프리팹입니다.")]
        public GameObject ItemPrefab;
        
        [Tooltip("파이프 생성 시 아이템이 함께 생성될 확률입니다 (0.0 ~ 1.0).")]
        [Range(0.0f, 1.0f)] public float ItemSpawnChance = 0.5f;
    }
}
