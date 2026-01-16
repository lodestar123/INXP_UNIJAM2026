using UnityEngine;

namespace Utils
{
    /// <summary>
    /// A thread-safe, persistent singleton pattern for MonoBehaviours.
    /// Ensures only one instance of the singleton exists.
    /// </summary>
    /// <typeparam name="T">The type of the singleton.</typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        // Lock object for thread-safe access
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                // Thread-safe lock
                lock (_lock)
                {
                    // If the instance is already set, return it.
                    if (_instance != null)
                        return _instance;

                    // Search for an existing instance in the scene
                    _instance = FindFirstObjectByType<T>();

                    // If no instance is found, create a new one
                    if (_instance == null)
                    {
                        // Create a new GameObject to host the singleton component
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T) + " (Singleton)";
                    }
                    
                    return _instance;
                }
            }
        }
        
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}