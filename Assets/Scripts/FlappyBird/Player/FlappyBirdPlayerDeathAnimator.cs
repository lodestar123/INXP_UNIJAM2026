using DG.Tweening;
using UnityEngine;

namespace FlappyBird.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FlappyBirdPlayerDeathAnimator : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Collider2D _collider;
        private Sequence _deathAnimationSequence;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        public void Play(TweenCallback onComplete)
        {
            Cancel();

            if (_rb is not null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.bodyType = RigidbodyType2D.Kinematic;
            }

            if (_collider is not null)
            {
                _collider.enabled = false;
            }

            Vector3 currentPos = transform.position;

            _deathAnimationSequence = DOTween.Sequence();
            _deathAnimationSequence
                .Append(transform.DOMoveY(currentPos.y + 1.5f, 0.4f).SetEase(Ease.OutQuad))
                .Join(transform.DORotate(new Vector3(0, 0, -120), 0.6f))
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

        public void Cancel()
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
