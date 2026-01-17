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

        public void ExpandPool(GameObject prefab, int amount)
        {
            if (!_poolDictionary.ContainsKey(prefab))
            {
                CreatePool(prefab, 0);
            }

            var objectQueue = _poolDictionary[prefab];
            for (int i = 0; i < amount; i++)
            {
                GameObject newObj = Instantiate(prefab, transform, true);
                newObj.SetActive(false);
                objectQueue.Enqueue(newObj);
            }
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(prefab))
            {
                CreatePool(prefab, 1);
            }

            Queue<GameObject> objectQueue = _poolDictionary[prefab];

            GameObject objectToSpawn = null;

            while (objectQueue.Count > 0)
            {
                objectToSpawn = objectQueue.Dequeue();
                if (objectToSpawn != null) break;
            }

            if (objectToSpawn == null)
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
            instance.transform.SetParent(transform);
            _poolDictionary[prefab].Enqueue(instance);
        }
    }
}