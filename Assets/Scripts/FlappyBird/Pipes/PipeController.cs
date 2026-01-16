using FlappyBird.Configs;
using UnityEngine;
using Utils;

namespace FlappyBird.Pipes
{
    public class PipeController : MonoBehaviour
    {
        public GameObject OriginalPrefab { get; set; }

        private FlappyBirdConfig _config;
        private float _offScreenX = -15f; // A default off-screen position
        
        public void SetConfig(FlappyBirdConfig config)
        {
            _config = config;
            
            if (_config)
            {
                _offScreenX = -_config.PipeSpawnX - 5.0f; 
            }
        }

        private void Update()
        {
            if (!_config) return;

            transform.position += Vector3.left * (_config.PipeMoveSpeed * Time.deltaTime);

            if (transform.position.x < _offScreenX)
            {
                if (OriginalPrefab != null)
                {
                    ObjectPool.Instance.Return(OriginalPrefab, gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
