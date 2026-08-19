using DG.Tweening;
using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적 우주선. 위치는 고정이며, 발사 시점의 플레이어 위치를 향한
    /// 직선 탄을 발사. 플레이어 레이저를 맞으면 체력이 줄고, 죽으면 아이템을 떨어뜨림.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GalagaEnemy : MonoBehaviour
    {
        private GalagaConfig _config;
        private GalagaGameManager _owner;
        private GalagaEnemyType _type;
        private SpriteRenderer _renderer;

        private int _hp;
        private float _fireTimer;
        private bool _dead;
        private float _holdY;
        private bool _holding;
        private bool _tweensPausedByGame;

        public bool IsDead => _dead;

        public void Initialize(
            GalagaConfig config,
            GalagaGameManager owner,
            GalagaEnemyType type,
            int hp,
            float holdY,
            SpriteRenderer renderer)
        {
            _config = config;
            _owner = owner;
            _type = type;
            _hp = Mathf.Max(1, hp);
            _holdY = holdY;
            _holding = false;
            _renderer = renderer;
            _dead = false;
            _tweensPausedByGame = false;

            _fireTimer = Mathf.Max(0f, _config.enemyFirstFireDelay);

            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            col.enabled = true;

            PlayEntry();
        }

        private void PlayEntry()
        {
            Vector3 holdPos = transform.position;
            holdPos.y = _holdY;

            float dropHeight = Mathf.Max(0.6f, _config.enemyEntryDropHeight);
            transform.position = holdPos + Vector3.up * dropHeight;
            transform.localScale = Vector3.one * 0.55f;

            if (_renderer != null)
            {
                _renderer.DOKill();
                Color c = _renderer.color;
                c.a = 0.7f;
                _renderer.color = c;
            }

            float duration = Mathf.Max(0.16f, _config.enemyEntryDuration);
            Sequence seq = DOTween.Sequence();
            seq.SetTarget(transform);
            seq.SetUpdate(false);
            seq.SetLink(gameObject);
            seq.Join(transform.DOMove(holdPos, duration).SetEase(Ease.OutBack, 1.55f));
            seq.Join(transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack, 1.7f));
            if (_renderer != null)
            {
                seq.Join(_renderer.DOFade(1f, duration * 0.4f));
            }
            seq.OnComplete(() =>
            {
                if (_dead) return;
                transform.position = holdPos;
                transform.localScale = Vector3.one;
                _holding = true;
            });
            SyncTweensWithPause();
        }

        private void Update()
        {
            SyncTweensWithPause();

            if (_dead || _config == null) return;

            if (GalagaGameManager.IsGameplayFrozen)
            {
                return;
            }

            if (!_holding)
            {
                return;
            }

            // 자리잡은 뒤 선택적으로 서서히 내려가도록 (기본 0 = 완전 고정)
            if (_config.enemyDescendSpeed > 0f)
            {
                transform.position += Vector3.down * (_config.enemyDescendSpeed * Time.deltaTime);
                if (transform.position.y <= _config.bottomDespawnY)
                {
                    // 화면 밖으로 지나간 적은 그냥 제거 (놓친 것으로 처리)
                    DespawnSilently();
                    return;
                }
            }

            _fireTimer -= Time.deltaTime;
            if (_fireTimer <= 0f)
            {
                FireAimedShot();
                ScheduleNextShot(0f);
            }
        }

        private void ScheduleNextShot(float extraDelay)
        {
            float elapsed = _owner != null ? _owner.ElapsedTime : 0f;
            float increaseInterval = Mathf.Max(0.01f, _config.fireRateIncreaseInterval);
            int steps = Mathf.FloorToInt(elapsed / increaseInterval);
            float currentInterval = Mathf.Max(
                _config.minEnemyFireInterval,
                _config.enemyFireInterval - steps * _config.fireIntervalDecreasePerStep);
            float jitter = Random.Range(0f, Mathf.Max(0f, _config.enemyFireIntervalRandom));
            _fireTimer = extraDelay + currentInterval + jitter;
        }

        private void FireAimedShot()
        {
            Vector2 origin = transform.position + Vector3.down * 0.4f;
            Vector2 dir = Vector2.down;

            GalagaPlayerController player = _owner != null ? _owner.Player : null;
            if (player != null && player.IsAlive)
            {
                dir = (Vector2)player.transform.position - origin;
                if (dir.sqrMagnitude < 0.0001f)
                {
                    dir = Vector2.down;
                }
                else
                {
                    dir.Normalize();
                }
            }

            GalagaEnemyBullet.Spawn(
                _owner != null ? _owner.ProjectilePool : null,
                _config,
                _type != null ? _type.bulletSprite : null,
                origin,
                dir);
        }

        public void TakeDamage(int amount)
        {
            if (_dead) return;

            _hp -= Mathf.Max(1, amount);
            FlashHit();

            if (_hp <= 0)
            {
                Die();
            }
        }

        private void FlashHit()
        {
            if (_renderer == null) return;
            _renderer.DOKill();
            _renderer.color = Color.white;
            Color target = _type != null ? _type.bodyColor : Color.white;
            _renderer.DOColor(target, 0.15f).SetUpdate(false).SetLink(gameObject);

            if (_holding)
            {
                transform.DOKill();
                transform.localScale = Vector3.one;
                transform.DOPunchScale(Vector3.one * 0.15f, 0.15f, 6, 0.5f).SetUpdate(false).SetLink(gameObject);
            }

            SyncTweensWithPause();
        }

        private void Die()
        {
            if (_dead) return;
            _dead = true;

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            _owner?.HandleEnemyKilled(transform.position);

            if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
            {
                GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ThreeMatch);
            }

            transform.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.SetTarget(transform);
            seq.SetUpdate(false);
            seq.SetLink(gameObject);
            seq.Append(transform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
            if (_renderer != null)
            {
                _renderer.DOKill();
                seq.Join(_renderer.DOFade(0f, 0.25f));
            }
            seq.OnComplete(() =>
            {
                _owner?.NotifyEnemyRemoved(this);
                Destroy(gameObject);
            });
            SyncTweensWithPause();
        }

        private void DespawnSilently()
        {
            _dead = true;
            _owner?.NotifyEnemyRemoved(this);
            transform.DOKill();
            if (_renderer != null) _renderer.DOKill();
            Destroy(gameObject);
        }

        private void OnDisable()
        {
            transform.DOKill();
            if (_renderer != null) _renderer.DOKill();
            _tweensPausedByGame = false;
        }

        // 일시정지/게임오버 중 등장/피격/사망 연출이 끝나지 않게 트윈을 멈춤
        private void SyncTweensWithPause()
        {
            bool shouldPause = GalagaGameManager.IsGameplayFrozen;
            if (shouldPause)
            {
                transform.DOPause();
                if (_renderer != null) _renderer.DOPause();
                _tweensPausedByGame = true;
                return;
            }

            if (_tweensPausedByGame)
            {
                transform.DOPlay();
                if (_renderer != null) _renderer.DOPlay();
                _tweensPausedByGame = false;
            }
        }
    }
}
