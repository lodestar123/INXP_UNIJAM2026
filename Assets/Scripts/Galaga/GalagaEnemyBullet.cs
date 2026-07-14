using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적이 던지는 사무용품(탄막) 하나
    /// </summary>
    [RequireComponent(typeof(Collider2D))]  
    [RequireComponent(typeof(Rigidbody2D))]
    public class GalagaEnemyBullet : MonoBehaviour
    {
        private Vector2 _velocity;
        private float _despawnY;

        public void Initialize(Vector2 direction, float speed, float despawnY)
        {
            _velocity = direction.normalized * speed;
            _despawnY = despawnY;

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

            transform.position += (Vector3)(_velocity * Time.deltaTime);

            if (transform.position.y <= _despawnY)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<GalagaPlayerController>();
            if (player == null || !player.IsAlive) return;

            player.Hit();
            Destroy(gameObject);
        }
    }
}
