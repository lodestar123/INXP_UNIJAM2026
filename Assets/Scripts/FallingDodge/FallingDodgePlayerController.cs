using Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace FallingDodge
{
    public class FallingDodgePlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float minX = -2.8f;
        [SerializeField] private float maxX = 2.8f;
        [SerializeField] private bool allowKeyboardInEditor = true;

        private bool _canMove;

        private void Awake()
        {
            _canMove = true;
        }

        private void OnEnable()
        {
            _canMove = true;
        }

        public void ResetState()
        {
            _canMove = true;
        }

        public void StopMovement()
        {
            _canMove = false;
        }

        private void Update()
        {
            if (!_canMove)
            {
                return;
            }

            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            float direction = ReadHorizontalDirection();
            if (Mathf.Approximately(direction, 0f))
            {
                return;
            }

            Vector3 position = transform.position;
            position.x += direction * moveSpeed * Time.deltaTime;
            position.x = Mathf.Clamp(position.x, minX, maxX);
            transform.position = position;
        }

        private float ReadHorizontalDirection()
        {
            if (allowKeyboardInEditor && Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                {
                    CustomLog.Info("asdasdasd");
                    return -1f;
                }

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                {
                    CustomLog.Info("asdasdasd");
                    return 1f;
                }
            }

            if (UnifiedInputManager.Instance == null || !UnifiedInputManager.Instance.IsPressing)
            {
                return 0f;
            }

            return UnifiedInputManager.Instance.PointerPosition.x < Screen.width * 0.5f ? -1f : 1f;
        }
    }
}
