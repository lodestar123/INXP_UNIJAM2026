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
        [Tooltip("애니메이션 비주얼 프리팹(자식 SpriteRenderer + Animator). playerSprite가 비어 있을 때 사용합니다.")]
        public GameObject playerVisualPrefab;
        [Tooltip("정지 스프라이트. 할당 시 playerVisualPrefab보다 우선해 루트 SpriteRenderer에 적용합니다.")]
        public Sprite playerSprite;
        [Tooltip("플레이어 레이저 스프라이트")]
        public Sprite laserSprite;
        [Tooltip("배경 타일(세로로 이어 붙이는 타일)")]
        public Sprite backgroundTileSprite;
        [Tooltip("Item.spritePast가 없을 때 드랍 아이템에 쓸 fallback 스프라이트")]
        public Sprite itemDropFallbackSprite;

        [Header("발사체 프리팹")]
        [Tooltip("플레이어 레이저 프리팹 (SpriteRenderer + CapsuleCollider2D + Rigidbody2D)")]
        public GameObject playerLaserPrefab;
        [Tooltip("적 총알 프리팹 (SpriteRenderer + CircleCollider2D + Rigidbody2D)")]
        public GameObject enemyBulletPrefab;

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
        [Tooltip("등장 연출 시간(초). 짧을수록 빠르게 내려옵니다.")]
        public float enemyEntryDuration = 0.38f;
        [Tooltip("자리 위쪽에서 내려오기 시작하는 높이")]
        public float enemyEntryDropHeight = 1.7f;
        [Tooltip("이 Y 아래로 내려간 오브젝트는 화면 밖으로 판단해 제거")]
        public float bottomDespawnY = -5.5f;

        [Header("플레이어")]
        [Tooltip("좌우 터치/키보드로 이동할 때의 최대 속도")]
        public float playerMoveSpeed = 12f;
        [Tooltip("가감속 시간(초). 작을수록 입력에 더 정직하게 반응합니다.")]
        public float playerMoveSmoothTime = 0.08f;
        [Tooltip("좌우 이동 시 기체가 기울어지는 최대 각도")]
        public float playerBankAngle = 10f;
        [Tooltip("플레이어 레이저 자동 발사 간격(초)")]
        public float playerFireInterval = 0.35f;
        [Tooltip("플레이어 레이저의 위로 이동 속도")]
        public float laserSpeed = 16f;
        [Tooltip("레이저 한 발이 적에게 주는 데미지")]
        public int laserDamage = 1;

        [Header("적 체력/공격")]
        [Tooltip("적의 기본 체력 (레이저 데미지 1 기준 2면 두 번 맞고 죽음")]
        public int enemyBaseHp = 2;
        [Tooltip("적이 아래로 내려오는 속도 (0이면 위치 고정)")]
        public float enemyDescendSpeed = 0f;
        [Tooltip("적의 기본 공격 간격(초)")]
        public float enemyFireInterval = 1.85f;
        [Tooltip("자리잡은 뒤 첫 공격을 하기까지의 대기 시간(초)")]
        public float enemyFirstFireDelay = 0.4f;
        [Tooltip("공격 간격에 더해지는 무작위 편차")]
        public float enemyFireIntervalRandom = 0.7f;
        [Tooltip("이 시간(초)이 지날 때마다 적 공격 간격이 줄어듦")]
        public float fireRateIncreaseInterval = 12f;
        [Tooltip("공격 속도 증가 시 한 번에 줄어드는 발사 간격(초)")]
        public float fireIntervalDecreasePerStep = 0.25f;
        [Tooltip("적 공격 간격의 최솟값(초)")]
        public float minEnemyFireInterval = 0.7f;
        [Tooltip("적 총알의 이동 속도")]
        public float enemyBulletSpeed = 5.0f;

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
        [Tooltip("아이템이 화면 아래로 떨어지는 DOTween 연출 시간(초)")]
        public float itemFallDuration = 0.65f;
        [Tooltip("적 처치 시 떨어지는 아이템 간 가로 간격")]
        public float itemDropHorizontalSpacing = 1.6f;

        [Header("배경")]
        [Tooltip("배경이 아래로 흐르는 속도")]
        public float backgroundScrollSpeed = 2.2f;

        [Header("점수")]
        [Tooltip("적을 처치할 때 얻는 점수")]
        public int scorePerKill = 100;

        [Header("적 종류")]
        public List<GalagaEnemyType> enemyTypes = new List<GalagaEnemyType>
        {
            new GalagaEnemyType { typeName = "클립병" },
            new GalagaEnemyType { typeName = "압정병" },
            new GalagaEnemyType { typeName = "스테이플러병" }
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
        [Tooltip("피격 연출 후 복원할 색. 스프라이트 원색을 쓰려면 흰색")]
        public Color bodyColor = Color.white;
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
