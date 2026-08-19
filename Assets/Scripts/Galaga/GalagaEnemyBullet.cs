using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적이 던지는 사무용품(탄막) 하나. 생성 시점의 방향으로 직선 이동한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GalagaEnemyBullet : MonoBehaviour
    {
        private const float OffScreenPadding = 1f;

        private Vector2 _velocity;
        private float _despawnY;
        private bool _consumed;
        private GalagaProjectilePool _pool;

        // 조준 방향으로 적탄 한 발을 풀에서 꺼냄
        public static void Spawn(
            GalagaProjectilePool pool,
            GalagaConfig config,
            Sprite sprite,
            Vector2 origin,
            Vector2 direction)
        {
            if (pool == null || config == null || sprite == null) return;

            pool.SpawnBullet(origin, direction, config.enemyBulletSpeed, config.bottomDespawnY, sprite);
        }

        public void Initialize(Vector2 direction, float speed, float despawnY, GalagaProjectilePool pool)
        {
            _velocity = direction.normalized * speed;
            _despawnY = despawnY;
            _consumed = false;
            _pool = pool;

            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            col.enabled = true;

            if (_velocity.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            if (GalagaGameManager.IsGameplayFrozen)
            {
                return;
            }

            transform.position += (Vector3)_velocity * Time.deltaTime;

            if (IsOffScreen())
            {
                Despawn();
            }
        }

        private bool IsOffScreen()
        {
            Vector3 p = transform.position;
            if (p.y <= _despawnY)
            {
                return true;
            }

            Camera cam = Camera.main;
            if (cam == null || !cam.orthographic)
            {
                return false;
            }

            float halfH = cam.orthographicSize + OffScreenPadding;
            float halfW = cam.orthographicSize * cam.aspect + OffScreenPadding;
            Vector3 camPos = cam.transform.position;
            return p.x < camPos.x - halfW
                || p.x > camPos.x + halfW
                || p.y < camPos.y - halfH
                || p.y > camPos.y + halfH;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;

            var player = other.GetComponent<GalagaPlayerController>();
            if (player == null || !player.IsAlive) return;

            _consumed = true;
            player.Hit();
            Despawn();
        }

        private void Despawn()
        {
            if (_pool != null)
            {
                _pool.ReturnBullet(this);
                return;
            }

            Destroy(gameObject);
        }
    }
}
