using DG.Tweening;
using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 적을 처치했을 때 아래로 떨어지는 아이템
    /// 스폰 직후 살짝 위로 튀어 올랐다가 가속하며 화면 아래로 낙하하고, 연출 종료 시 애니팡 아이템 큐에 자동 추가
    /// </summary>
    public class GalagaItemPickup : MonoBehaviour
    {
        private const float BounceHeight = 0.38f;
        private const float BounceDuration = 0.11f;

        private GalagaGameManager _owner;
        private Item _item;
        private float _fallDuration;
        private float _despawnY;
        private bool _isExiting;
        private bool _itemQueued;
        private SpriteRenderer _renderer;
        private Tween _fallTween;
        private bool _pausedByGame;

        public void Initialize(GalagaGameManager owner, Item item, float fallDuration, float despawnY, SpriteRenderer renderer)
        {
            _owner = owner;
            _item = item;
            _fallDuration = fallDuration;
            _despawnY = despawnY;
            _isExiting = false;
            _itemQueued = false;
            _pausedByGame = false;
            _renderer = renderer;

            StartFallAnimation();
        }

        private void Update()
        {
            if (_fallTween == null || !_isExiting) return;

            bool shouldPause = GalagaGameManager.IsGameplayFrozen;

            if (shouldPause)
            {
                if (!_pausedByGame && _fallTween.IsActive() && _fallTween.IsPlaying())
                {
                    _fallTween.Pause();
                    _pausedByGame = true;
                }

                return;
            }

            if (_pausedByGame && _fallTween.IsActive())
            {
                _fallTween.Play();
                _pausedByGame = false;
            }
        }

        private void StartFallAnimation()
        {
            if (_isExiting) return;
            _isExiting = true;

            float targetY = _despawnY - 2f;
            float fallDuration = Mathf.Max(0.2f, _fallDuration);

            transform.DOKill();
            var sequence = DOTween.Sequence();

            sequence.Append(
                transform.DOLocalMoveY(BounceHeight, BounceDuration)
                    .SetRelative()
                    .SetEase(Ease.OutQuad));

            sequence.Append(
                transform.DOMoveY(targetY, fallDuration)
                    .SetEase(Ease.InQuad)
                    .OnUpdate(TryQueueItemWhenOffScreen));

            if (_renderer != null)
            {
                _renderer.DOKill();
                sequence.Join(_renderer.DOFade(0f, fallDuration * 0.45f).SetDelay(fallDuration * 0.35f));
            }

            sequence.OnComplete(FinishAndDestroy);
            sequence.SetUpdate(false);
            sequence.SetLink(gameObject);
            _fallTween = sequence;
            if (GalagaGameManager.IsGameplayFrozen && _fallTween.IsActive())
            {
                _fallTween.Pause();
                _pausedByGame = true;
            }
        }

        private void TryQueueItemWhenOffScreen()
        {
            if (_itemQueued || transform.position.y > _despawnY) return;

            _itemQueued = true;
            _owner?.HandleItemOffScreen(_item);
        }

        private void FinishAndDestroy()
        {
            if (!_itemQueued)
            {
                _owner?.HandleItemOffScreen(_item);
            }

            Destroy(gameObject);
        }

        private void OnDisable()
        {
            transform.DOKill();
            if (_renderer != null) _renderer.DOKill();
            _fallTween = null;
            _pausedByGame = false;
        }
    }
}
