using Core.Input;
using FlappyBird.Game;
using FlappyBird.Interfaces.Player;
using FlappyBird.Components;
using UnityEngine;
using DG.Tweening;

namespace FlappyBird.Player
{
    // 플레이어의 입력 처리와 충돌 감지를 담당하는 메인 클래스입니다.
    [RequireComponent(typeof(IFlappyBirdPlayerMotor))]
    public class FlappyBirdPlayer : MonoBehaviour
    {
        private IFlappyBirdPlayerMotor _motor;
        private IBirdInputSource _input;
        private Rigidbody2D _rb;
        
        private bool _isPlayerActive = false;
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

            // 모터에 현재 입력 상태 전달
            _motor.MotorFixedTick(_input.IsHolding);
        }

        // 장애물(파이프)과의 물리적 충돌 처리
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_isPlayerActive) return;
            
            Debug.Log($"[충돌] {collision.gameObject.name}");

            if (collision.gameObject.CompareTag("Pipe"))
            {
                Debug.Log("파이프 충돌!");
            }

            // 게임 로직 종료 (점수 계산 중단, 스폰 중단)
            FlappyBirdGameManager.Instance.EndGame(); 
            DeactivatePlayer();

            // 사망 애니메이션 재생 후 씬 전환
            PlayDeathAnimation(() => FlappyBirdGameManager.Instance.TransitionToNextGame());
        }

        // 아이템과의 트리거 충돌 처리
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isPlayerActive) return;

            if (!other.CompareTag("Item")) return;
            
            if (other.TryGetComponent(out WorldItem worldItem))
            {
                Debug.Log($"[아이템] {worldItem.ItemData.name} 획득");
                FlappyItemCollector.CollectItem(worldItem.ItemData);
            }
            else
            {
                Debug.Log($"[아이템] {other.gameObject.name} 획득 (데이터 없음)");
            }
                
            other.gameObject.SetActive(false);
        }

        // 플레이어 동작을 활성화합니다.
        public void ActivatePlayer()
        {
            _isPlayerActive = true;
            
            // 물리 시뮬레이션 활성화
            if (_rb is not null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
            }
            
            // 등장 애니메이션 중단 (혹시 진행 중이라면)
            transform.DOKill();
            IsAnimating = false;
        }

        // 플레이어 동작을 비활성화합니다.
        public void DeactivatePlayer()
        {
            _isPlayerActive = false;
        }
        
        // 플레이어 상태를 리셋합니다.
        public void ResetPlayer()
        {
            if (_motor == null)
            {
                _motor = GetComponent<IFlappyBirdPlayerMotor>();
            }

            // 물리 시뮬레이션 비활성화 (Ready 상태에서 중력 영향 받지 않도록)
            if (_rb is not null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.linearVelocity = Vector2.zero;
            }

            // 콜라이더 재활성화 (사망 시 꺼졌을 수 있음)
            var col = GetComponent<Collider2D>();
            if (col is not null) col.enabled = true;

            // 회전 초기화
            transform.rotation = Quaternion.identity;
            
            // 기존 애니메이션(사망 등) 중단
            transform.DOKill();

            _motor?.ResetState();
            DeactivatePlayer();
            
            // 등장 애니메이션: 아래에서 위로 떠오르기
            Vector3 startPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.position = startPos + Vector3.down * 3f; // 아래쪽에서 시작
            
            IsAnimating = true; // 애니메이션 시작
            transform.DOMove(startPos, 0.4f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => IsAnimating = false); // 애니메이션 종료 시 플래그 해제
        }

        private void PlayDeathAnimation(TweenCallback onComplete)
        {
            // 물리 제어권 가져오기 (애니메이션을 위해)
            if (_rb is not null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.bodyType = RigidbodyType2D.Kinematic; 
            }
            
            // 추가 충돌 방지
            var col = GetComponent<Collider2D>();
            if (col is not null) col.enabled = false;

            // 사망 애니메이션: 위로 튀어올랐다가 아래로 추락
            Sequence seq = DOTween.Sequence();
            Vector3 currentPos = transform.position;
            
            // 1. 위로 살짝 튀어오름 (Bounce) + 회전
            seq.Append(transform.DOMoveY(currentPos.y + 1.5f, 0.4f).SetEase(Ease.OutQuad))
               .Join(transform.DORotate(new Vector3(0, 0, -120), 0.6f)) // 머리가 아래로 향하게 회전
            // 2. 아래로 추락
               .AppendCallback(() =>
               {
                   GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.Die);
               })
               .Append(transform.DOMoveY(currentPos.y - 12f, 0.8f).SetEase(Ease.InBack))
               .OnComplete(onComplete);
        }
    }
}