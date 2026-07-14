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
        private Transform _laserParent;

        private bool _canMove;
        private bool _isFiring;
        private float _fireTimer;
        private float _dragOffsetX;
        private bool _hasDragAnchor;

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
            _hasDragAnchor = false;
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
        }

        public void ResetPlayer()
        {
            IsAlive = true;
            _canMove = false;
            _isFiring = false;
            _fireTimer = 0f;
            _hasDragAnchor = false;

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

            if (GameSceneManager.Instance != null &&
                (GameSceneManager.Instance.IsPaused || GameSceneManager.Instance.IsGameOver))
            {
                return;
            }

            if (!_canMove) return;

            HandleMovement();
            HandleFiring();
        }

        private void HandleMovement()
        {
            float keyboardDir = ReadKeyboardDirection();
            if (!Mathf.Approximately(keyboardDir, 0f))
            {
                _hasDragAnchor = false;
                MoveByDirection(keyboardDir);
                return;
            }

            if (TryReadPointerTargetX(out float targetX))
            {
                MoveTowardX(targetX);
            }
            else
            {
                _hasDragAnchor = false;
            }
        }

        private void MoveByDirection(float direction)
        {
            GalagaPlayArea.GetHorizontalBounds(_config, _camera, out float minX, out float maxX);

            Vector3 pos = transform.position;
            pos.x += direction * _config.playerMoveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }

        private void MoveTowardX(float targetX)
        {
            GalagaPlayArea.GetHorizontalBounds(_config, _camera, out float minX, out float maxX);

            Vector3 pos = transform.position;
            float maxStep = _config.playerMoveSpeed * Time.deltaTime;
            pos.x = Mathf.MoveTowards(pos.x, targetX, maxStep);
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
        }

        /// <summary>
        /// 포인터(터치/마우스)를 누르고 있을 때, 손가락을 좌우로 움직인 만큼 우주선을
        /// 상대적으로 이동시키기 위한 목표 X를 계산 (스와이프/드래그 조종)
        /// </summary>
        private bool TryReadPointerTargetX(out float targetX)
        {
            targetX = transform.position.x;

            bool pressing = UnifiedInputManager.Instance != null
                ? UnifiedInputManager.Instance.IsPressing
                : (Pointer.current != null && Pointer.current.press.isPressed);

            if (!pressing)
            {
                return false;
            }

            Vector2 screenPos = UnifiedInputManager.Instance != null
                ? UnifiedInputManager.Instance.PointerPosition
                : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

            float pointerWorldX = ScreenToWorldX(screenPos.x);

            if (!_hasDragAnchor)
            {
                // 누르기 시작한 순간의 손가락 위치와 우주선 위치 차이를 기억해
                // 화면 어디를 눌러도 튀지 않고 상대 이동하도록 함
                _dragOffsetX = transform.position.x - pointerWorldX;
                _hasDragAnchor = true;
            }

            targetX = pointerWorldX + _dragOffsetX;
            return true;
        }

        private float ScreenToWorldX(float screenX)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return transform.position.x;
            }

            float depth = Mathf.Abs(_camera.transform.position.z - transform.position.z);
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screenX, _camera.pixelHeight * 0.5f, depth));
            return world.x;
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
