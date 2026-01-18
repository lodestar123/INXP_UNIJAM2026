using UnityEngine;

namespace FlappyBird
{
    public class BackgroundScrolling : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("파이프 속도 대비 배경 이동 속도 비율 (예: 0.5는 파이프 속도의 절반)")]
        [SerializeField] private float parallaxFactor = 0.1f; 

        [Tooltip("텍스처 UV 스크롤 속도 보정값 (텍스처 반복 주기 조절용)")]
        [SerializeField] private float uvSpeedMultiplier = 0.1f; 

        private Material _material;
        
        private void Awake()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _material = spriteRenderer.material;
            }
            else
            {
                Debug.LogWarning("BackgroundScrolling: SpriteRenderer가 없습니다.");
            }
        }

        private void Update()
        {
            if (!_material) return;
            if (!PipeSpawner.IsScrolling) return;

            // 매 프레임 최신 파이프 속도를 가져옵니다.

            float currentSpeed = PipeSpawner.CurrentScrollSpeed;
            
            float offsetChange = currentSpeed * Time.deltaTime * parallaxFactor * uvSpeedMultiplier;
            
            _material.mainTextureOffset += new Vector2(offsetChange, 0);
        }
    }
}
