using Core.Input;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Galaga
{
    /// <summary>
    /// 스테이지 4 플레이어
    /// </summary>
    public class GalagaPlayerController : MonoBehaviour
    {
        private GalagaConfig _config;
        private GalagaGameManager _owner;
        private Camera _camera;

        private bool _canMove;
        private bool _isFiring;
        private float _fireTimer;
        private float _velocityX;
        private float _velocityXDamp;
        private float _bankZ;
        private float _bankZDamp;
        private bool _blockPointerUntilRelease;

        public bool IsAlive { get; private set; } = true;

        public void Initialize(
            GalagaConfig config,
            GalagaGameManager owner,
            Camera cam)
        {
            _config = config;
            _owner = owner;
            _camera = cam != null ? cam : Camera.main;
            IsAlive = true;
            _canMove = false;
            _isFiring = false;
            _fireTimer = 0f;
            _blockPointerUntilRelease = false;
            ResetMotionFeel();
        }

        public void StartPlaying()
        {
            _canMove = true;
            _isFiring = true;
            _fireTimer = 0f;
            _blockPointerUntilRelease = IsPointerHeld();
        }

        public void StopPlaying()
        {
            _canMove = false;
            _isFiring = false;
            _blockPointerUntilRelease = false;
            _velocityX = 0f;
            _velocityXDamp = 0f;
        }

        public void ResetPlayer()
        {
            IsAlive = true;
            _canMove = false;
            _isFiring = false;
            _fireTimer = 0f;
            _blockPointerUntilRelease = false;
            ResetMotionFeel();

            transform.DOKill();
            transform.rotation = Quaternion.identity;

            if (_config != null)
            {
                Vector3 pos = transform.position;
                pos.x = 0f;
                pos.y = _config.playerY;
                pos.z = 0f;
                transform.position = pos;
            }

            var body = GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.bodyType = RigidbodyType2D.Kinematic;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }

        public void Hit()
        {
            if (!IsAlive) return;
            IsAlive = false;
            _canMove = false;
            _isFiring = false;

            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            _owner?.HandlePlayerDeath();
        }

        private void Update()
        {
            if (_config == null) return;

            if (GalagaGameManager.IsGameplayFrozen)
            {
                return;
            }

            if (GameSceneManager.Instance != null && GameSceneManager.Instance.IsInputGateActive)
            {
                return;
            }

            if (!_canMove) return;

            HandleMovement();
            HandleFiring();
        }

        private void HandleMovement()
        {
            float direction = ReadHorizontalDirection();
            bool keyboardControl = !Mathf.Approximately(ReadKeyboardDirection(), 0f);
            if (keyboardControl || (!_blockPointerUntilRelease && !Mathf.Approximately(direction, 0f)))
            {
                _owner?.NotifyPlayerControlStarted();
            }

            if (_blockPointerUntilRelease && !IsPointerHeld())
            {
                _blockPointerUntilRelease = false;
            }

            float maxSpeed = _config.playerMoveSpeed;
            float targetSpeed = direction * maxSpeed;
            float smoothTime = Mathf.Max(0.02f, _config.playerMoveSmoothTime);
            _velocityX = Mathf.SmoothDamp(_velocityX, targetSpeed, ref _velocityXDamp, smoothTime);

            GalagaPlayArea.GetHorizontalBounds(_config, _camera, out float minX, out float maxX);

            Vector3 pos = transform.position;
            pos.x += _velocityX * Time.deltaTime;
            if (pos.x <= minX)
            {
                pos.x = minX;
                if (_velocityX < 0f)
                {
                    _velocityX = 0f;
                    _velocityXDamp = 0f;
                }
            }
            else if (pos.x >= maxX)
            {
                pos.x = maxX;
                if (_velocityX > 0f)
                {
                    _velocityX = 0f;
                    _velocityXDamp = 0f;
                }
            }
            transform.position = pos;

            float speedRatio = Mathf.Clamp(maxSpeed > 0.01f ? _velocityX / maxSpeed : 0f, -1f, 1f);
            float targetBank = -speedRatio * _config.playerBankAngle;
            _bankZ = Mathf.SmoothDamp(_bankZ, targetBank, ref _bankZDamp, smoothTime * 1.15f);
            transform.rotation = Quaternion.Euler(0f, 0f, _bankZ);
        }

        private void ResetMotionFeel()
        {
            _velocityX = 0f;
            _velocityXDamp = 0f;
            _bankZ = 0f;
            _bankZDamp = 0f;
        }

        private float ReadHorizontalDirection()
        {
            float keyboardDirection = ReadKeyboardDirection();
            if (!Mathf.Approximately(keyboardDirection, 0f))
            {
                return keyboardDirection;
            }

            if (UnifiedInputManager.Instance != null && UnifiedInputManager.Instance.IsPressing)
            {
                return GetDirectionFromScreenX(UnifiedInputManager.Instance.PointerPosition.x);
            }

            return 0f;
        }

        private float ReadKeyboardDirection()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return 0f;

            bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;

            if (left == right) return 0f;
            return left ? -1f : 1f;
        }

        private static bool IsPointerHeld()
        {
            if (UnifiedInputManager.Instance != null && UnifiedInputManager.Instance.IsPressing)
            {
                return true;
            }

            return Pointer.current != null && Pointer.current.press.isPressed;
        }

        private static float GetDirectionFromScreenX(float screenX)
        {
            return screenX < Screen.width * 0.5f ? -1f : 1f;
        }

        private void HandleFiring()
        {
            if (!_isFiring) return;

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            _fireTimer = Mathf.Max(0.05f, _config.playerFireInterval);
            GalagaLaser.Spawn(_owner != null ? _owner.ProjectilePool : null, _config, transform.position);
        }
    }
}
