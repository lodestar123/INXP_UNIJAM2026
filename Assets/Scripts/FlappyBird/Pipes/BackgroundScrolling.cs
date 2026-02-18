using FlappyBird.Interfaces.Pipes;
using UnityEngine;

namespace FlappyBird
{
    /// <summary>
    /// 파이프 속도에 맞춰 배경 텍스처를 패럴랙스 스크롤합니다.
    /// </summary>
    public class BackgroundScrolling : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("파이프 속도 대비 배경 이동 속도 비율 (예: 0.5는 파이프 속도의 절반)")]
        [SerializeField] private float parallaxFactor = 0.1f; 

        [Tooltip("텍스처 UV 스크롤 속도 보정값 (텍스처 반복 주기 조절용)")]
        [SerializeField] private float uvSpeedMultiplier = 0.1f; 

        [SerializeField] private MonoBehaviour speedProviderSource;

        private Material _material;
        private IScrollSpeedProvider _speedProvider;
        
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

            _speedProvider = ResolveSpeedProvider();
        }

        private void Update()
        {
            if (!_material) return;
            _speedProvider ??= ResolveSpeedProvider();
            if (_speedProvider == null || !_speedProvider.IsScrolling) return;

            // 현재 스크롤 속도를 기준으로 UV 오프셋 갱신
            float currentSpeed = _speedProvider.CurrentScrollSpeed;
            
            float offsetChange = currentSpeed * Time.deltaTime * parallaxFactor * uvSpeedMultiplier;
            
            _material.mainTextureOffset += new Vector2(offsetChange, 0);
        }

        private IScrollSpeedProvider ResolveSpeedProvider()
        {
            if (speedProviderSource is IScrollSpeedProvider typed)
            {
                return typed;
            }

            return FindAnyObjectByType<PipeSpawner>();
        }
    }
}
