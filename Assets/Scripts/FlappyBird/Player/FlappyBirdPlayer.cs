using Core.Input;
using FlappyBird.Game;
using FlappyBird.Interfaces.Player;
using FlappyBird.Components;
using UnityEngine;
using DG.Tweening;

namespace FlappyBird.Player
{
    // Main player controller handling input, movement, and collisions.
    [RequireComponent(typeof(IFlappyBirdPlayerMotor))]
    public class FlappyBirdPlayer : MonoBehaviour
    {
        private IFlappyBirdPlayerMotor _motor;
        private IBirdInputSource _input;
        private Rigidbody2D _rb;

        private bool _isPlayerActive = false;

        public bool IsPlayerActive
        {
            get => _isPlayerActive;
        }

        public bool IsAnimating { get; private set; } = false;

        private void Awake()
        {
            _motor = GetComponent<IFlappyBirdPlayerMotor>();
            _input = GetComponent<IBirdInputSource>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!_isPlayerActive || _input is null) return;

            // Forward current input state to motor.
            _motor.MotorFixedTick(_input.IsHolding);
        }

        // Handle physics collisions with world objects.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_isPlayerActive) return;

            Debug.Log($"[Collision] {collision.gameObject.name}");

            if (collision.gameObject.CompareTag("Pipe"))
            {
                Debug.Log("Hit pipe!");
            }

            // End game logic (stop score, stop spawns).
            FlappyBirdGameManager.Instance.EndGame();
            DeactivatePlayer();

            // Play death animation then transition.
            PlayDeathAnimation(() => FlappyBirdGameManager.Instance.TransitionToNextGame());
        }

        // Handle trigger pickups (items).
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isPlayerActive) return;

            if (!other.CompareTag("Item")) return;

            if (other.TryGetComponent<FlappyBird.Game.FlappyBirdItem>(out _))
            {
                return; // FlappyBirdItem handles itself.
            }

            if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
            {
                GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.GetItem);
            }

            if (other.TryGetComponent(out WorldItem worldItem))
            {
                // WorldItem uses AnimateCollect with internal duplicate checks.
                Debug.Log($"[Item] Collected: {worldItem.ItemData.name}");
                FlappyItemCollector.CollectItem(worldItem.ItemData);
                worldItem.AnimateCollect();
            }
            else
            {
                Debug.Log($"[Item] Collected: {other.gameObject.name} (no data)");
                other.gameObject.SetActive(false);
            }
        }

        // Activate player movement and physics.
        public void ActivatePlayer()
        {
            _isPlayerActive = true;

            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody2D>();
            }

            // Enable physics body.
            if (_rb is not null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
            }

            // Stop any intro animation.
            transform.DOKill();
            IsAnimating = false;

            if (GameSceneManager.Instance is not null)
            {
                GameSceneManager.Instance.ResumeGame();
            }
        }

        public void DeactivatePlayer()
        {
            _isPlayerActive = false;
        }

        // Reset player state to Ready.
        public void ResetPlayer()
        {
            if (_motor == null)
            {
                _motor = GetComponent<IFlappyBirdPlayerMotor>();
            }

            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody2D>();
            }

            // Disable physics during Ready state.
            if (_rb is not null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.linearVelocity = Vector2.zero;
            }

            // Re-enable collider.
            var col = GetComponent<Collider2D>();
            if (col is not null) col.enabled = true;

            // Reset rotation.
            transform.rotation = Quaternion.identity;

            // Stop any previous animations.
            transform.DOKill();

            _motor?.ResetState();
            DeactivatePlayer();

            // Intro move up animation.
            Vector3 startPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.position = startPos + Vector3.down * 6f;

            IsAnimating = true;
            transform
                .DOMove(startPos, 0.4f)
                .SetDelay(1.0f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => IsAnimating = false);
        }

        private Sequence _deathAnimationSequence;

        private void PlayDeathAnimation(TweenCallback onComplete)
        {
            CancelDeathAnimation();

            // Take control of physics for animation.
            if (_rb is not null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.bodyType = RigidbodyType2D.Kinematic;
            }

            // Prevent additional collisions.
            var col = GetComponent<Collider2D>();
            if (col is not null) col.enabled = false;

            // Death animation: pop up then fall.
            _deathAnimationSequence = DOTween.Sequence();
            Vector3 currentPos = transform.position;

            // 1) Move up (bounce) + rotate.
            _deathAnimationSequence
                .Append(transform.DOMoveY(currentPos.y + 1.5f, 0.4f).SetEase(Ease.OutQuad))
                .Join(transform.DORotate(new Vector3(0, 0, -120), 0.6f)) // Rotate nose down.
            // 2) Fall down.
               .AppendCallback(() =>
               {
                   if (GameManager.Instance != null && GameManager.Instance.soundManager != null)
                   {
                       GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.Die);
                   }
               })
               .Append(transform.DOMoveY(currentPos.y - 12f, 0.8f).SetEase(Ease.InBack))
               .OnComplete(onComplete);
        }
        // Called on game start: cancel any death animation.
        public void CancelDeathAnimation()
        {
            if (_deathAnimationSequence != null)
            {
                _deathAnimationSequence.Kill();
                _deathAnimationSequence = null;
            }

            transform.DOKill();
        }
    }
}

