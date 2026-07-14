using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 스테이지 4 프로토타입의 모든 튜닝 값을 담는 설정 에셋
    /// </summary>
    [CreateAssetMenu(fileName = "GalagaConfig", menuName = "Scriptable Objects/GalagaConfig")]
    public class GalagaConfig : ScriptableObject
    {
        [Header("스프라이트")]
        [Tooltip("애니메이션 비주얼 프리팹(자식 SpriteRenderer + Animator). 할당 시 playerSprite보다 우선합니다.")]
        public GameObject playerVisualPrefab;
        [Tooltip("정지 스프라이트. playerVisualPrefab이 없을 때 루트 SpriteRenderer에 적용합니다.")]
        public Sprite playerSprite;
        [Tooltip("플레이어 레이저 스프라이트")]
        public Sprite laserSprite;
        [Tooltip("배경 타일(세로로 이어 붙이는 타일)")]
        public Sprite backgroundTileSprite;
        [Tooltip("Item.spritePast가 없을 때 드랍 아이템에 쓸 fallback 스프라이트")]
        public Sprite itemDropFallbackSprite;

        [Header("플레이 영역 (월드 좌표)")]
        [Tooltip("카메라 좌우 가장자리에서 안쪽으로 여백 (카메라 기준 이동 범위 계산용)")]
        public float playAreaHorizontalPadding = 0.3f;
        [Tooltip("카메라를 찾지 못할 때만 사용하는 fallback 최소 X")]
        public float playAreaMinX = -2.8f;
        [Tooltip("카메라를 찾지 못할 때만 사용하는 fallback 최대 X")]
        public float playAreaMaxX = 2.8f;
        [Tooltip("플레이어가 위치하는 Y 좌표")]
        public float playerY = -3.0f;
        [Tooltip("화면 위쪽 기준 Y 좌표 (레이저 소멸 상한 등에 사용)")]
        public float topY = 4.8f;
        [Tooltip("적이 처음 생성되는 Y 좌표 (화면 위(보이는 영역 밖)에서 등장시킴)")]
        public float enemySpawnY = 5.8f;
        [Tooltip("적이 진입 후 자리잡는 Y 범위의 최소값 (보이는 영역 상단)")]
        public float enemyHoldYMin = 1.4f;
        [Tooltip("적이 진입 후 자리잡는 Y 범위의 최대값")]
        public float enemyHoldYMax = 3.8f;
        [Tooltip("적이 진입할 때 자리잡을 때까지의 하강 속도")]
        public float enemyEntrySpeed = 4.0f;
        [Tooltip("이 Y 아래로 내려간 오브젝트는 화면 밖으로 판단해 제거")]
        public float bottomDespawnY = -5.5f;

        [Header("플레이어")]
        [Tooltip("스와이프/키보드로 좌우 이동할 때의 최대 속도")]
        public float playerMoveSpeed = 12f;
        [Tooltip("플레이어 레이저 자동 발사 간격(초)")]
        public float playerFireInterval = 0.35f;
        [Tooltip("플레이어 레이저의 위로 이동 속도")]
        public float laserSpeed = 16f;
        [Tooltip("레이저 한 발이 적에게 주는 데미지")]
        public int laserDamage = 1;

        [Header("적 체력/공격")]
        [Tooltip("적의 기본 체력 (레이저 데미지 1 기준 2면 두 번 맞고 죽음")]
        public int enemyBaseHp = 2;
        [Tooltip("이 시간(초)이 지날 때마다 새로 생성되는 적의 체력이 증가")]
        public float hpIncreaseInterval = 12f;
        [Tooltip("체력 증가 시 한 번에 오르는 양")]
        public int hpIncreasePerStep = 1;
        [Tooltip("시간이 흘러도 적 체력이 이 값을 넘지 않음")]
        public int maxEnemyHp = 8;
        [Tooltip("적이 아래로 내려오는 속도 (0이면 위치 고정)")]
        public float enemyDescendSpeed = 0f;
        [Tooltip("적의 기본 공격 간격(초)")]
        public float enemyFireInterval = 2.2f;
        [Tooltip("공격 간격에 더해지는 무작위 편차")]
        public float enemyFireIntervalRandom = 0.7f;
        [Tooltip("부채꼴 탄막 한 번에 발사되는 총알 수")]
        public int fanBulletCount = 5;
        [Tooltip("부채꼴 탄막의 전체 벌어짐 각도(도) (넓을수록 피하기 쉬움)")]
        public float fanSpreadAngle = 110f;
        [Tooltip("적 총알의 이동 속도")]
        public float enemyBulletSpeed = 4.5f;

        [Header("적 생성")]
        [Tooltip("동시에 존재할 수 있는 최대 적 수")]
        public int maxAliveEnemies = 4;
        [Tooltip("적이 새로 생성되는 간격(초)")]
        public float enemySpawnInterval = 1.6f;
        [Tooltip("게임 시작 후 첫 적이 나오기까지의 대기 시간")]
        public float initialSpawnDelay = 1.0f;
        [Tooltip("적이 배치될 가로 레인(칸) 수")]
        public int laneCount = 5;

        [Header("아이템 드랍")]
        [Tooltip("적을 처치했을 때 떨어지는 최소 아이템 수")]
        public int minDropCount = 1;
        [Tooltip("적을 처치했을 때 떨어지는 최대 아이템 수")]
        public int maxDropCount = 3;
        [Tooltip("떨어지는 아이템의 낙하 속도")]
        public float itemFallSpeed = 3.4f;

        [Header("배경")]
        [Tooltip("배경이 아래로 흐르는 속도")]
        public float backgroundScrollSpeed = 2.2f;

        [Header("점수")]
        [Tooltip("적을 처치할 때 얻는 점수")]
        public int scorePerKill = 100;

        [Header("적 종류")]
        public List<GalagaEnemyType> enemyTypes = new List<GalagaEnemyType>
        {
            new GalagaEnemyType
            {
                typeName = "클립병",
                bodyColor = new Color(0.90f, 0.35f, 0.35f),
                fireIntervalMultiplier = 1f,
                fanBulletCountOverride = 0
            },
            new GalagaEnemyType
            {
                typeName = "압정병",
                bodyColor = new Color(0.45f, 0.65f, 0.95f),
                fireIntervalMultiplier = 1.25f,
                fanBulletCountOverride = 3
            },
            new GalagaEnemyType
            {
                typeName = "스테이플러병",
                bodyColor = new Color(0.6f, 0.85f, 0.45f),
                fireIntervalMultiplier = 0.85f,
                fanBulletCountOverride = 7
            }
        };
    }

    /// <summary>
    /// 적 종류 하나에 대한 데이터.
    /// </summary>
    [Serializable]
    public class GalagaEnemyType
    {
        public string typeName = "Enemy";
        public Sprite enemySprite;
        public Sprite bulletSprite;
        [Tooltip("피격 연출 후 복원할 색 (스프라이트 틴트용, 기본 흰색)")]
        public Color bodyColor = Color.white;
        [Tooltip("이 종류의 공격 간격 배율 (1보다 크면 더 느리게 공격")]
        public float fireIntervalMultiplier = 1f;
        [Tooltip("이 종류만 부채꼴 총알 수를 다르게 하려면 0보다 큰 값을 넣습니다. (0이면 공용 값을 사용)")]
        public int fanBulletCountOverride = 0;
    }

    public static class GalagaPlayArea
    {
        public static void GetHorizontalBounds(GalagaConfig config, Camera camera, out float minX, out float maxX)
        {
            float padding = config != null ? config.playAreaHorizontalPadding : 0.3f;

            if (camera != null && camera.orthographic)
            {
                float halfWidth = camera.orthographicSize * camera.aspect;
                minX = -halfWidth + padding;
                maxX = halfWidth - padding;
                return;
            }

            minX = config != null ? config.playAreaMinX : -2.8f;
            maxX = config != null ? config.playAreaMaxX : 2.8f;
        }
    }
}
