using UnityEngine;

namespace FlappyBird.Components
{
    // 오브젝트를 지정된 방향과 속도로 직선 이동시키는 클래스입니다.
    public class LinearMover : MonoBehaviour
    {
        private Vector3 _direction = Vector3.left;
        private float _speed = 0f;
        private bool _isInitialized = false;

        // 이동 데이터를 초기화합니다.
        public void Initialize(Vector3 direction, float speed)
        {
            _direction = direction;
            _speed = speed;
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            float moveAmount = _speed * Time.deltaTime;
            transform.position += _direction * moveAmount;
        }
    }
}