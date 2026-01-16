using UnityEngine;
using Utils;

namespace FlappyBird.Components
{
    // 오브젝트가 특정 경계(Boundary)를 벗어났을 때 비활성화하거나 파괴하는 클래스입니다.
    // 단일 책임 원칙(SRP): 오직 '수명 주기 관리'에만 집중합니다.
    public class BoundaryRecycler : MonoBehaviour
    {
        private float _xThreshold;
        private GameObject _originalPrefab;
        private bool _isInitialized = false;

        // 재활용 데이터를 초기화합니다.
        public void Initialize(float xThreshold, GameObject originalPrefab)
        {
            _xThreshold = xThreshold;
            _originalPrefab = originalPrefab;
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            if (transform.position.x < _xThreshold)
            {
                RecycleObject();
            }
        }

        private void RecycleObject()
        {
            if (_originalPrefab is not null)
            {
                // 오브젝트 풀 시스템을 통해 반납
                ObjectPool.Instance.Return(_originalPrefab, gameObject);
            }
            else
            {
                // 풀링 대상이 아니면 파괴
                Destroy(gameObject);
            }
        }
    }
}