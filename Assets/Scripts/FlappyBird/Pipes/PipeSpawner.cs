using FlappyBird.Configs;
using FlappyBird.Pipes;
using UnityEngine;
using Utils;

namespace FlappyBird
{
    public class PipeSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Assign the main configuration file for the game.")]
        [SerializeField] private FlappyBirdConfig config;

        [Header("Prefab")]
        [Tooltip("Assign the Pipe prefab to be spawned.")]
        [SerializeField] private GameObject pipePrefab;

        private bool _isSpawning = false;
        private float _timer = 0f;
        
        private void Start()
        {
            if (config == null || pipePrefab == null)
            {
                Debug.LogError("Configuration or Pipe Prefab is not set in PipeSpawner.", this);
                enabled = false; // Disable this component if not configured properly
                return;
            }
            
            ObjectPool.Instance.CreatePool(pipePrefab, 5);
        }

        private void Update()
        {
            if (!_isSpawning) return;

            _timer += Time.deltaTime;
            if (_timer >= config.PipeSpawnInterval)
            {
                _timer = 0f;
                SpawnPipe();
            }
        }

        private void SpawnPipe()
        {
            // Determine random spawn height
            float randomY = Random.Range(config.PipeMinY, config.PipeMaxY);
            Vector3 spawnPosition = new Vector3(config.PipeSpawnX, randomY, 0);

            // Spawn the pipe using the ObjectPool
            GameObject pipeInstance = ObjectPool.Instance.Spawn(pipePrefab, spawnPosition, Quaternion.identity);

            // Configure the spawned pipe
            PipeController pipeController = pipeInstance.GetComponent<PipeController>();
            if (pipeController != null)
            {
                pipeController.OriginalPrefab = pipePrefab;
                pipeController.SetConfig(config); // Pass the config to the pipe
            }
        }

        /// <summary>
        /// Starts the pipe spawning process. Called by GameManager.
        /// </summary>
        public void StartSpawning()
        {
            _isSpawning = true;
            _timer = 0f; // Reset timer to spawn a pipe almost immediately
        }

        /// <summary>
        /// Stops the pipe spawning process. Called by GameManager.
        /// </summary>
        public void StopSpawning()
        {
            _isSpawning = false;
        }

        /// <summary>
        /// Deactivates all currently active pipes and returns them to the pool.
        /// Called by GameManager on restart.
        /// </summary>
        public void ClearPipes()
        {
            // This is a simple way to find all pipes. For a large number of objects,
            // it would be better for the spawner to keep a list of its active objects.
            PipeController[] activePipes = FindObjectsByType<PipeController>(FindObjectsSortMode.None);
            foreach (var pipe in activePipes)
            {
                ObjectPool.Instance.Return(pipe.OriginalPrefab, pipe.gameObject);
            }
        }
    }
}
