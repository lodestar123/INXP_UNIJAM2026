using DG.Tweening;
using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적 우주선. 위치는 고정이며, 주기적으로 부채꼴 탄막을
    /// 아래로 발사. 플레이어 레이저를 맞으면 체력이 줄고, 죽으면 아이템을 떨어뜨림.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GalagaEnemy : MonoBehaviour
    {
        private GalagaConfig _config;
        private GalagaGameManager _owner;
        private GalagaEnemyType _type;
        private SpriteRenderer _renderer;
        private Transform _bulletParent;

        private int _hp;
        private float _fireTimer;
        private bool _dead;
        private float _holdY;
        private bool _holding;

        public bool IsDead => _dead;

        public void Initialize(
            GalagaConfig config,
            GalagaGameManager owner,
            GalagaEnemyType type,
            int hp,
            float holdY,
            SpriteRenderer renderer,
            Transform bulletParent = null)
        {
            _config = config;
            _owner = owner;
            _type = type;
            _hp = Mathf.Max(1, hp);
            _holdY = holdY;
            _holding = false;
            _renderer = renderer;
            _bulletParent = bulletParent;
            _dead = false;

            ScheduleNextShot(0.3f);

            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            col.enabled = true;

            transform.localScale = Vector3.one;
            if (_renderer != null)
            {
                _renderer.DOKill();
                Color c = _renderer.color;
                c.a = 1f;
                _renderer.color = c;
            }
        }

        private void Update()
        {
            if (_dead || _config == null) return;

            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            if (!_holding)
            {
                // 화면 위에서 진입해 지정된 Y에 도달하면 자리잡고 고정됩니다.
                float step = _config.enemyEntrySpeed * Time.deltaTime;
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, _holdY, step);
                transform.position = pos;

                if (Mathf.Approximately(pos.y, _holdY) || pos.y <= _holdY)
                {
                    _holding = true;
                }
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
                FireFan();
                ScheduleNextShot(0f);
            }
        }

        private void ScheduleNextShot(float extraDelay)
        {
            float baseInterval = _config.enemyFireInterval * (_type != null ? Mathf.Max(0.1f, _type.fireIntervalMultiplier) : 1f);
            float jitter = Random.Range(0f, Mathf.Max(0f, _config.enemyFireIntervalRandom));
            _fireTimer = extraDelay + baseInterval + jitter;
        }

        private void FireFan()
        {
            int count = _config.fanBulletCount;
            if (_type != null && _type.fanBulletCountOverride > 0)
            {
                count = _type.fanBulletCountOverride;
            }
            count = Mathf.Max(1, count);

            Vector2 origin = transform.position + Vector3.down * 0.4f;

            // 아래 방향(-90도)을 중심으로 부채꼴 전개
            float center = -90f;
            float spread = Mathf.Max(0f, _config.fanSpreadAngle);
            float start = center - spread * 0.5f;
            float step = count > 1 ? spread / (count - 1) : 0f;

            for (int i = 0; i < count; i++)
            {
                float angleDeg = count > 1 ? start + step * i : center;
                float rad = angleDeg * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                SpawnBullet(origin, dir);
            }
        }

        private void SpawnBullet(Vector2 origin, Vector2 direction)
        {
            if (_type == null || _type.bulletSprite == null) return;

            var go = new GameObject("EnemyBullet");
            if (_bulletParent != null) go.transform.SetParent(_bulletParent, true);
            go.transform.position = origin;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _type.bulletSprite;
            sr.color = Color.white;
            sr.sortingOrder = 4;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.16f;
            col.isTrigger = true;

            var bullet = go.AddComponent<GalagaEnemyBullet>();
            bullet.Initialize(direction, _config.enemyBulletSpeed, _config.bottomDespawnY);
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
            _renderer.DOColor(target, 0.15f);

            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * 0.15f, 0.15f, 6, 0.5f);
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
        }
    }
}
