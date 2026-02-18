using FlappyBird.Interfaces.Pipes;
using UnityEngine;

namespace FlappyBird.Components
{
    /// <summary>
    /// 오브젝트를 지정 방향으로 일정하게 이동시킵니다.
    /// </summary>
    public class LinearMover : MonoBehaviour
    {
        private Vector3 _direction = Vector3.left;
        private bool _isInitialized = false;

        private bool _useProviderSpeed = true;
        private float _localSpeed = 0f;
        private IScrollSpeedProvider _scrollSpeedProvider;

        // 이동 방향, 초기 속도, 속도 공급자를 설정합니다.
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
