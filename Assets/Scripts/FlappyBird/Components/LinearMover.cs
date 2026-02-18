using FlappyBird.Interfaces.Pipes;
using UnityEngine;

namespace FlappyBird.Components
{
    // 오브젝트를 지정된 방향과 속도로 직선 이동시키는 클래스입니다.
    public class LinearMover : MonoBehaviour
    {
        private Vector3 _direction = Vector3.left;
        private bool _isInitialized = false;

        private bool _useProviderSpeed = true;
        private float _localSpeed = 0f;
        private IScrollSpeedProvider _scrollSpeedProvider;

        // 이동 데이터를 초기화합니다.
        public void Initialize(Vector3 direction, float speed, IScrollSpeedProvider scrollSpeedProvider)
        {
            _direction = direction;
            _localSpeed = speed;
            _isInitialized = true;
            _scrollSpeedProvider = scrollSpeedProvider;

            _useProviderSpeed = speed > 0f;
        }

        public void SetMoveState(bool isMoving)
        {
            _useProviderSpeed = isMoving;
            if (!isMoving) _localSpeed = 0f;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            float currentSpeed = _useProviderSpeed && _scrollSpeedProvider != null
                ? _scrollSpeedProvider.CurrentScrollSpeed
                : _localSpeed;

            float moveAmount = currentSpeed * Time.deltaTime;
            transform.position += _direction * moveAmount;
        }
    }
}
