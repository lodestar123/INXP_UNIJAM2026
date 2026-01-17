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
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 플레이어와 충돌했는지 확인
            if (other.CompareTag("Player"))
            {
                // 게임 매니저에게 아이템 획득 알림
                if (FlappyBirdGameManager.Instance != null)
                {
                    FlappyBirdGameManager.Instance.OnItemCollected(itemData);
                }
            }
        }
    }
}