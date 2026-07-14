using DG.Tweening;
using FlappyBird.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Galaga
{
    /// <summary>
    /// 우주선 슈팅 진행을 담당하는 매니저
    /// </summary>
    public class GalagaGameManager : MonoBehaviour
    {
        [SerializeField] private GalagaConfig config;
        [SerializeField] private GalagaPlayerController player;
        [SerializeField] private GalagaEnemySpawner spawner;
        [SerializeField] private GalagaBackgroundScroller background;

        [Header("단독 실행(테스트 씬)용")]
        [SerializeField] private bool autoStartInStandalone = true;
        [Tooltip("자동 시작까지의 대기 시간(초)입니다.")]
        [SerializeField] private float autoStartDelay = 1.2f;

        private bool _isEnding;
        private bool _isReadyToStart;
        private bool _isPlaying;
        private FlappyBirdPlayerDeathAnimator _deathAnimator;

        public GalagaConfig Config => config;

        public void Configure(
            GalagaConfig cfg,
            GalagaPlayerController playerController,
            GalagaEnemySpawner enemySpawner,
            GalagaBackgroundScroller bg)
        {
            config = cfg;
            player = playerController;
            spawner = enemySpawner;
            background = bg;
        }

        private void OnEnable()
        {
            BeginGame();
        }

        private void Start()
        {
            if (GameSceneManager.Instance != null && GameSceneManager.Instance.isActiveAndEnabled)
            {
                return;
            }

            BeginGame();
        }

        private void OnDisable()
        {
            spawner?.StopSpawning();
            player?.StopPlaying();
        }

        private void Update()
        {
            if (_isReadyToStart && IsStartPressedThisFrame())
            {
                StartGame();
            }
        }

        private void BeginGame()
        {
            if (config == null || player == null || spawner == null)
            {
                // 부트스트랩이 아직 구성 요소를 연결하지 않은 경우
                return;
            }

            _isEnding = false;
            _isReadyToStart = false;
            _isPlaying = false;

            EnsureDeathAnimator();
            _deathAnimator?.Cancel();

            spawner.ResetState();
            player.ResetPlayer();

            EnterReadyState();
        }

        private void EnterReadyState()
        {
            if (_isEnding || (GameSceneManager.Instance != null && GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            player?.StopPlaying();
            _isReadyToStart = true;

            // 단독 실행 시엔 탭 안내 UI가 없으므로 잠시 후 자동 시작
            if (autoStartInStandalone && GameSceneManager.Instance == null)
            {
                DOVirtual.DelayedCall(Mathf.Max(0f, autoStartDelay), () =>
                {
                    if (_isReadyToStart && !_isPlaying && !_isEnding)
                    {
                        StartGame();
                    }
                });
            }
        }

        private void StartGame()
        {
            if (!_isReadyToStart || _isPlaying) return;

            _isReadyToStart = false;
            _isPlaying = true;
            player?.StartPlaying();
            spawner?.StartSpawning();
        }

        private bool IsStartPressedThisFrame()
        {
            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver || GameSceneManager.Instance.IsTransitioning))
            {
                return false;
            }

            return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        }

        private void EnsureDeathAnimator()
        {
            if (player == null) return;

            if (_deathAnimator == null)
            {
                _deathAnimator = player.GetComponent<FlappyBirdPlayerDeathAnimator>();
            }
        }

        // ---------- 게임 진입/이탈 (GameSceneManager 연동용) ----------

        public void OnEnterGame()
        {
            BeginGame();
        }

        public void OnExitGame()
        {
            _isReadyToStart = false;
            _isPlaying = false;
            spawner?.StopSpawning();
            player?.StopPlaying();
        }

        // ---------- 게임 이벤트 ----------

        public void HandleItemCollected(Item item)
        {
            if (item == null) return;

            FlappyItemCollector.CollectItem(item);

            if (GameSceneManager.Instance != null && config != null)
            {
                GameSceneManager.Instance.AddItem(item.value);
            }

            if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
            {
                GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.GetItem);
            }
        }

        public void HandleEnemyKilled(Vector3 position)
        {
            spawner?.SpawnItemDrops(position);

            if (GameSceneManager.Instance != null && config != null)
            {
                GameSceneManager.Instance.AddScore(config.scorePerKill);
            }
        }

        public void NotifyEnemyRemoved(GalagaEnemy enemy)
        {
            spawner?.UnregisterEnemy(enemy);
        }

        public void HandlePlayerDeath()
        {
            if (_isEnding) return;

            _isEnding = true;
            _isReadyToStart = false;
            _isPlaying = false;
            spawner?.StopSpawning();
            player?.StopPlaying();

            EnsureDeathAnimator();

            TweenCallback complete = OnDeathSequenceComplete;

            if (_deathAnimator != null && player != null)
            {
                _deathAnimator.Play(complete);
                return;
            }

            // 사망 애니메이터가 없으면 잠깐 대기 후 처리함
            DOVirtual.DelayedCall(0.6f, () => complete());
        }

        private void OnDeathSequenceComplete()
        {
            // 듀얼 게임 구조: 과거(스테이지) 게임에서 죽으면 현재(애니팡)로 전환
            if (GameSceneManager.Instance != null)
            {
                if (GameSceneManager.Instance.CurrentGameId == 1)
                {
                    GameSceneManager.Instance.OnChangeGame();
                }
                return;
            }

            // 단독 실행: 잠시 후 재시작
            DOVirtual.DelayedCall(0.8f, BeginGame);
        }
    }
}
