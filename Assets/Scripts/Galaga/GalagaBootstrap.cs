using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 배경/플레이어/스포너/매니저 오브젝트 생성 및 초기화
    /// </summary>
    public class GalagaBootstrap : MonoBehaviour
    {
        [SerializeField] private GalagaConfig config;
        [SerializeField] private bool manageCamera = true;
        [SerializeField] private float cameraOrthographicSize = 5.0f;

        private bool _built;

        private void Start()
        {
            Build();
        }

        private void Build()
        {
            if (_built) return;
            _built = true;
            BuildPrototype();
        }

        private void BuildPrototype()
        {
            GalagaConfig activeConfig = config != null
                ? config
                : ScriptableObject.CreateInstance<GalagaConfig>();

            Camera cam = ResolveCamera();

            // 전환 시 함께 정리되도록 투사체/적을 담을 컨테이너
            var projectilesRoot = new GameObject("Projectiles");
            projectilesRoot.transform.SetParent(transform, false);

            GalagaBackgroundScroller background = CreateBackground(activeConfig);
            GalagaPlayerController player = CreatePlayer(activeConfig, cam, projectilesRoot.transform);
            GalagaEnemySpawner spawner = CreateSpawner(projectilesRoot.transform);

            var managerGo = new GameObject("GalagaGameManager");
            managerGo.transform.SetParent(transform, false);
            var manager = managerGo.AddComponent<GalagaGameManager>();

            player.Initialize(activeConfig, manager, cam, projectilesRoot.transform);
            spawner.Initialize(activeConfig, manager, projectilesRoot.transform);
            manager.Configure(activeConfig, player, spawner, background);

            manager.OnEnterGame();
        }

        private Camera ResolveCamera()
        {
            Camera cam = Camera.main;

            if (!manageCamera)
            {
                return cam;
            }

            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }

            cam.orthographic = true;
            cam.orthographicSize = cameraOrthographicSize;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.backgroundColor = new Color(0.03f, 0.02f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            return cam;
        }

        private GalagaBackgroundScroller CreateBackground(GalagaConfig cfg)
        {
            var go = new GameObject("GalagaBackground");
            go.transform.SetParent(transform, false);
            go.transform.position = Vector3.zero;
            var scroller = go.AddComponent<GalagaBackgroundScroller>();

            if (cfg.backgroundTileSprite == null)
            {
                Debug.LogWarning("[Galaga] GalagaConfig.backgroundTileSprite가 비어 있습니다.");
                return scroller;
            }

            float worldHeight = cameraOrthographicSize * 2f;
            scroller.Initialize(cfg.backgroundScrollSpeed, cfg.backgroundTileSprite, worldHeight, -100);
            return scroller;
        }

        private GalagaPlayerController CreatePlayer(GalagaConfig cfg, Camera cam, Transform laserParent)
        {
            var go = new GameObject("GalagaPlayer");
            go.transform.SetParent(transform, true);
            go.transform.position = new Vector3(0f, cfg.playerY, 0f);

            SetupPlayerVisual(go, cfg);

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.32f;
            col.isTrigger = true;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.simulated = true;

            go.AddComponent<FlappyBird.Player.FlappyBirdPlayerDeathAnimator>();

            return go.AddComponent<GalagaPlayerController>();
        }

        /// <summary>
        /// FallingDodge처럼 루트(물리/충돌) + 자식 비주얼(Animator/SpriteRenderer) 구조를 지원합니다.
        /// </summary>
        private static void SetupPlayerVisual(GameObject playerRoot, GalagaConfig cfg)
        {
            if (cfg.playerVisualPrefab != null)
            {
                var visual = Instantiate(cfg.playerVisualPrefab, playerRoot.transform);
                visual.name = cfg.playerVisualPrefab.name;
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;

                foreach (var sr in visual.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    sr.sortingOrder = 8;
                }
                return;
            }

            if (cfg.playerSprite != null)
            {
                var sr = playerRoot.AddComponent<SpriteRenderer>();
                sr.sprite = cfg.playerSprite;
                sr.color = Color.white;
                sr.sortingOrder = 8;
                return;
            }

            Debug.LogWarning("[Galaga] GalagaConfig에 playerVisualPrefab 또는 playerSprite를 할당하세요.");
        }

        private GalagaEnemySpawner CreateSpawner(Transform container)
        {
            var go = new GameObject("GalagaEnemySpawner");
            go.transform.SetParent(transform, false);
            return go.AddComponent<GalagaEnemySpawner>();
        }
    }
}
