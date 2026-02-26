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

        private static readonly object LOCK = new object();

        public static T Instance
        {
            get
            {
                lock (LOCK)
                {
                    if (_instance != null)
                        return _instance;

                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
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

                // DontDestroyOnLoad는 루트 오브젝트에서만 유효하므로
                // 싱글톤이 자식으로 배치된 경우 먼저 루트로 분리합니다.
                if (transform.parent != null)
                {
                    transform.SetParent(null);
                }

                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
