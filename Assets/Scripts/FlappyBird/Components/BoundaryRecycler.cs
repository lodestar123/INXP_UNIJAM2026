using UnityEngine;
using Utils;

namespace FlappyBird.Components
{
    /// <summary>
    /// 화면 경계를 지난 오브젝트를 풀에 반납하거나 제거합니다.
    /// </summary>
    public class BoundaryRecycler : MonoBehaviour
    {
        private float _xThreshold;
        private GameObject _originalPrefab;
        private bool _isInitialized = false;

        // 경계값과 원본 프리팹 정보를 설정합니다.
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
                // 풀링 대상이면 풀에 반납
                ObjectPool.Instance.Return(_originalPrefab, gameObject);
            }
            else
            {
                // 풀링 정보가 없으면 즉시 제거
                Destroy(gameObject);
            }
        }
    }
}
