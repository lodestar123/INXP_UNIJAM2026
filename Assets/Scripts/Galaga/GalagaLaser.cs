using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 플레이어가 발사하는 레이저
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class GalagaLaser : MonoBehaviour
    {
        private float _speed;
        private int _damage;
        private float _topBound;
        private bool _consumed;
        private GalagaProjectilePool _pool;

        // 플레이어 위치에서 레이저 한 발을 풀에서 꺼냄
        public static void Spawn(GalagaProjectilePool pool, GalagaConfig config, Vector3 playerPosition)
        {
            if (config == null || config.laserSprite == null)
            {
                Debug.LogWarning("[Galaga] GalagaConfig.laserSprite가 비어 있어 레이저를 발사하지 않습니다.");
                return;
            }

            if (pool == null) return;

            pool.SpawnLaser(
                playerPosition + Vector3.up * 0.6f,
                config.laserSpeed,
                config.laserDamage,
                config.topY + 2f,
                config.laserSprite);
        }

        public void Initialize(float speed, int damage, float topBound, GalagaProjectilePool pool)
        {
            _speed = speed;
            _damage = damage;
            _topBound = topBound;
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
        }

        private void Update()
        {
            if (GalagaGameManager.IsGameplayFrozen)
            {
                return;
            }

            transform.position += Vector3.up * (_speed * Time.deltaTime);

            if (transform.position.y >= _topBound)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;

            var enemy = other.GetComponent<GalagaEnemy>();
            if (enemy == null || enemy.IsDead) return;

            _consumed = true;
            enemy.TakeDamage(_damage);
            Despawn();
        }

        private void Despawn()
        {
            if (_pool != null)
            {
                _pool.ReturnLaser(this);
                return;
            }

            Destroy(gameObject);
        }
    }
}
