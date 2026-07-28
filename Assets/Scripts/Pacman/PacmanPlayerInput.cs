using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Pacman
{
    /// <summary>
    /// Stage3 팩맨 플레이어 입력을 읽고 이동 요청 방향을 저장함.
    /// </summary>
    public class PacmanPlayerInput : MonoBehaviour
    {
        [Header("Input")]
        // 이 거리 이상 드래그 시 스와이프로 인정함.
        [SerializeField] private float swipeThreshold = 60f;
        [SerializeField] private bool allowKeyboardInEditor = true;

        private PacmanConfig _config;
        private Vector2 _requestedDirection;
        private float _requestedDirectionTime = float.NegativeInfinity;
        private Vector2 _touchStartPosition;
        private Vector2 _mouseStartPosition;
        private int _activeTouchId = -1;
        private bool _hasTouchStartPosition;
        private bool _hasMouseStartPosition;
        private bool _canReadInput = true;

        public Vector2 RequestedDirection => _requestedDirection;
        public float RequestedDirectionTime => _requestedDirectionTime;

        public void Configure(PacmanConfig config)
        {
            _config = config;
        }

        private void OnEnable()
        {
            ResetState();
        }

        private void Update()
        {
            if (!_canReadInput || IsGameStopped())
            {
                return;
            }

            ReadKeyboardInput();
            ReadTouchSwipe();
            ReadMouseSwipe();
        }

        public void ResetState()
        {
            _canReadInput = true;
            ClearRequestedDirection();
            ResetInputState();
        }

        public void StopReadingInput()
        {
            _canReadInput = false;
            ClearRequestedDirection();
            ResetInputState();
        }

        public void ClearRequestedDirection()
        {
            _requestedDirection = Vector2.zero;
            _requestedDirectionTime = float.NegativeInfinity;
        }

        private void RequestDirection(Vector2 direction)
        {
            direction = ToCardinal(direction);

            if (direction == Vector2.zero)
            {
                return;
            }

            _requestedDirection = direction;
            _requestedDirectionTime = Time.time;
        }

        private void ReadKeyboardInput()
        {
            bool keyboardEnabled = _config != null ? _config.allowKeyboardInEditor : allowKeyboardInEditor;
            if (!keyboardEnabled || Keyboard.current == null)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                RequestDirection(Vector2.up);
            }
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                RequestDirection(Vector2.down);
            }
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                RequestDirection(Vector2.left);
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                RequestDirection(Vector2.right);
            }
        }

        private void ReadTouchSwipe()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                ResetTouchInput();
                return;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    _activeTouchId = touch.touchId.ReadValue();
                    _touchStartPosition = touch.position.ReadValue();
                    _hasTouchStartPosition = true;
                    return;
                }
            }

            if (_activeTouchId == -1 || !_hasTouchStartPosition)
            {
                return;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.touchId.ReadValue() != _activeTouchId)
                {
                    continue;
                }

                if (touch.press.isPressed)
                {
                    TryApplySwipe(touch.position.ReadValue() - _touchStartPosition);
                    return;
                }

                ResetTouchInput();
                return;
            }

            ResetTouchInput();
        }

        private void ReadMouseSwipe()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                ResetMouseInput();
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _mouseStartPosition = mouse.position.ReadValue();
                _hasMouseStartPosition = true;
                return;
            }

            if (!_hasMouseStartPosition)
            {
                return;
            }

            if (mouse.leftButton.isPressed)
            {
                TryApplySwipe(mouse.position.ReadValue() - _mouseStartPosition);
                return;
            }

            ResetMouseInput();
        }

        private void TryApplySwipe(Vector2 swipeDelta)
        {
            float threshold = _config != null ? _config.playerSwipeThreshold : swipeThreshold;
            if (swipeDelta.sqrMagnitude < threshold * threshold)
            {
                return;
            }

            RequestDirection(swipeDelta);
            ResetTouchInput();
            ResetMouseInput();
        }

        private static Vector2 ToCardinal(Vector2 direction)
        {
            if (direction == Vector2.zero)
            {
                return Vector2.zero;
            }

            // 더 크게 움직인 축 기준으로 4방향 입력만 허용함.
            return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? new Vector2(Mathf.Sign(direction.x), 0f)
                : new Vector2(0f, Mathf.Sign(direction.y));
        }

        private bool IsGameStopped()
        {
            bool mainGameStopped = GameSceneManager.Instance != null &&
                                   (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver);
            bool pacmanWaitingToStart = PacmanGameManager.Instance != null &&
                                        !PacmanGameManager.Instance.IsPlaying;

            return mainGameStopped || pacmanWaitingToStart;
        }

        private void ResetInputState()
        {
            ResetTouchInput();
            ResetMouseInput();
        }

        private void ResetTouchInput()
        {
            _activeTouchId = -1;
            _hasTouchStartPosition = false;
            _touchStartPosition = Vector2.zero;
        }

        private void ResetMouseInput()
        {
            _hasMouseStartPosition = false;
            _mouseStartPosition = Vector2.zero;
        }
    }
}
