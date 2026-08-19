using Core.Input;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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
        private Transform _laserParent;

        private bool _canMove;
        private bool _isFiring;
        private float _fireTimer;
        private int _activeTouchId = -1;
        private float _touchDirection;
        private float _velocityX;
        private float _velocityXDamp;
        private float _bankZ;
        private float _bankZDamp;

        public bool IsAlive { get; private set; } = true;

        public void Initialize(
            GalagaConfig config,
            GalagaGameManager owner,
            Camera cam,
            Transform laserParent = null)
        {
            _config = config;
            _owner = owner;
            _camera = cam != null ? cam : Camera.main;
            _laserParent = laserParent;
            IsAlive = true;
            _canMove = false;
            _isFiring = false;
            _fireTimer = 0f;
            ResetTouchDirection();
            ResetMotionFeel();
        }

        public void StartPlaying()
        {
            _canMove = true;
            _isFiring = true;
            _fireTimer = 0f;
        }

        public void StopPlaying()
        {
            _canMove = false;
            _isFiring = false;
            ResetTouchDirection();
            _velocityX = 0f;
            _velocityXDamp = 0f;
        }

        public void ResetPlayer()
        {
            IsAlive = true;
            _canMove = false;
            _isFiring = false;
            _fireTimer = 0f;
            ResetTouchDirection();
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

            if (!_canMove) return;

            HandleMovement();
            HandleFiring();
        }

        private void HandleMovement()
        {
            float direction = ReadHorizontalDirection();
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

            float touchDirection = ReadTouchDirection();
            if (!Mathf.Approximately(touchDirection, 0f))
            {
                return touchDirection;
            }

            if (UnifiedInputManager.Instance != null && UnifiedInputManager.Instance.IsPressing)
            {
                return GetDirectionFromScreenX(UnifiedInputManager.Instance.PointerPosition.x);
            }

            if (Pointer.current != null && Pointer.current.press.isPressed)
            {
                return GetDirectionFromScreenX(Pointer.current.position.ReadValue().x);
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

        private float ReadTouchDirection()
        {
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen == null)
            {
                ResetTouchDirection();
                return 0f;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    _activeTouchId = touch.touchId.ReadValue();
                    _touchDirection = GetDirectionFromScreenX(touch.position.ReadValue().x);
                }
            }

            if (_activeTouchId == -1)
            {
                return 0f;
            }

            foreach (TouchControl touch in touchscreen.touches)
            {
                if (touch.touchId.ReadValue() != _activeTouchId)
                {
                    continue;
                }

                if (touch.press.isPressed)
                {
                    _touchDirection = GetDirectionFromScreenX(touch.position.ReadValue().x);
                    return _touchDirection;
                }

                ResetTouchDirection();
                return 0f;
            }

            ResetTouchDirection();
            return 0f;
        }

        private static float GetDirectionFromScreenX(float screenX)
        {
            return screenX < Screen.width * 0.5f ? -1f : 1f;
        }

        private void ResetTouchDirection()
        {
            _activeTouchId = -1;
            _touchDirection = 0f;
        }

        private void HandleFiring()
        {
            if (!_isFiring) return;

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            _fireTimer = Mathf.Max(0.05f, _config.playerFireInterval);
            FireLaser();
        }

        private void FireLaser()
        {
            if (_config.laserSprite == null)
            {
                Debug.LogWarning("[Galaga] GalagaConfig.laserSprite가 비어 있어 레이저를 발사하지 않습니다.");
                return;
            }

            var go = new GameObject("PlayerLaser");
            if (_laserParent != null) go.transform.SetParent(_laserParent, true);
            go.transform.position = transform.position + Vector3.up * 0.6f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _config.laserSprite;
            sr.sortingOrder = 5;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Vertical;
            col.size = new Vector2(0.15f, 0.5f);
            col.isTrigger = true;

            var laser = go.AddComponent<GalagaLaser>();
            laser.Initialize(_config.laserSpeed, _config.laserDamage, _config.topY + 2f);
        }
    }
}
