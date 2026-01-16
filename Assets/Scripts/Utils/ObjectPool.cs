using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        private Dictionary<GameObject, Queue<GameObject>> _poolDictionary;

        protected override void Awake()
        {
            base.Awake();
            _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        }

        public void CreatePool(GameObject prefab, int initialSize)
        {
            if (_poolDictionary.ContainsKey(prefab))
            {
                return;
            }

            var objectQueue = new Queue<GameObject>();
            for (int i = 0; i < initialSize; i++)
            {
                GameObject newObj = Instantiate(prefab, transform, true);
                newObj.SetActive(false);
                objectQueue.Enqueue(newObj);
            }
            _poolDictionary.Add(prefab, objectQueue);
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(prefab))
            {
                CreatePool(prefab, 1);
            }

            Queue<GameObject> objectQueue = _poolDictionary[prefab];

            GameObject objectToSpawn;

            if (objectQueue.Count > 0)
            {
                objectToSpawn = objectQueue.Dequeue();
            }
            else
            {
                objectToSpawn = Instantiate(prefab, transform, true);
            }

            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            objectToSpawn.SetActive(true);

            return objectToSpawn;
        }

        public void Return(GameObject prefab, GameObject instance)
        {
            if (!_poolDictionary.ContainsKey(prefab))
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            _poolDictionary[prefab].Enqueue(instance);
        }
    }
}