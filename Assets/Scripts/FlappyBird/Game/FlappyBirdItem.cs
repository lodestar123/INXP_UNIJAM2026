using System;
using UnityEngine;

namespace FlappyBird.Game
{
    /// <summary>
    /// 플래피 버드 게임 내에서 생성되는 아이템을 관리하는 클래스입니다.
    /// 플레이어와 충돌 시 획득 처리를 담당합니다.
    /// </summary>
    public class FlappyBirdItem : MonoBehaviour
    {
        [Tooltip("이 오브젝트가 담고 있는 아이템 데이터입니다.")]
        public Item itemData;
        
        private bool _isCollected = false; // 중복 수집 방지 플래그
        
        private void Awake()
        {
            if (TryGetComponent<FlappyBird.Components.WorldItem>(out _))
            {
                enabled = false;
                return;
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 이미 수집되었거나 플레이어가 아니면 무시
            if (_isCollected || !other.CompareTag("Player")) return;
            
            // WorldItem 컴포넌트가 있으면 WorldItem이 처리하도록 함 (중복 방지)
            if (TryGetComponent<FlappyBird.Components.WorldItem>(out _))
            {
                return; // WorldItem이 처리하도록 함
            }
            
            _isCollected = true;
            
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            // 게임 매니저에게 아이템 획득 알림
            if (FlappyBirdGameManager.Instance != null)
            {
                FlappyBirdGameManager.Instance.OnItemCollected(itemData);
            }
            
            // 아이템 비활성화 (중복 충돌 방지)
            gameObject.SetActive(false);
        }
    }
}