using Core.Input;
using FlappyBird.Game;
using FlappyBird.Interfaces.Player;
using UnityEngine;

namespace FlappyBird.Player
{
    // 플레이어의 입력 처리와 충돌 감지를 담당하는 메인 클래스입니다.
    [RequireComponent(typeof(IFlappyBirdPlayerMotor))]
    public class FlappyBirdPlayer : MonoBehaviour
    {
        private IFlappyBirdPlayerMotor _motor;
        private IBirdInputSource _input;
        
        private bool _isPlayerActive = false;

        private void Awake()
        {
            _motor = GetComponent<IFlappyBirdPlayerMotor>();
            _input = GetComponent<IBirdInputSource>();
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
            if (_isPlayerActive)
            {
                Debug.Log($"[충돌] {collision.gameObject.name}");

                if (collision.gameObject.CompareTag("Pipe"))
                {
                    Debug.Log("파이프 충돌!");
                }

                FlappyBirdGameManager.Instance.EndGame(); 
                DeactivatePlayer();
            }
        }

        // 아이템과의 트리거 충돌 처리
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isPlayerActive) return;

            if (other.CompareTag("Item"))
            {
                Debug.Log($"[아이템] {other.gameObject.name} 획득");
                other.gameObject.SetActive(false);
            }
        }

        // 플레이어 동작을 활성화합니다.
        public void ActivatePlayer()
        {
            _isPlayerActive = true;
        }

        // 플레이어 동작을 비활성화합니다.
        public void DeactivatePlayer()
        {
            _isPlayerActive = false;
        }
        
        // 플레이어 상태를 리셋합니다.
        public void ResetPlayer()
        {
            _motor.ResetState();
            DeactivatePlayer();
        }
    }
}
