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

        public void Initialize(float speed, int damage, float topBound)
        {
            _speed = speed;
            _damage = damage;
            _topBound = topBound;
            _consumed = false;

            var body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            col.enabled = true;
        }

        private void Update()
        {
            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            transform.position += Vector3.up * (_speed * Time.deltaTime);

            if (transform.position.y >= _topBound)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_consumed) return;

            var enemy = other.GetComponent<GalagaEnemy>();
            if (enemy == null || enemy.IsDead) return;

            _consumed = true;
            enemy.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
