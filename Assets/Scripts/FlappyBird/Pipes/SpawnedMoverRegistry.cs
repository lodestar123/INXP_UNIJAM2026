using System.Collections.Generic;
using FlappyBird.Components;
using Utils;
using UnityEngine;

namespace FlappyBird
{
    public sealed class SpawnedMoverRegistry
    {
        private readonly List<LinearMover> _activeMovers = new List<LinearMover>();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefabMap = new Dictionary<GameObject, GameObject>();

        public void Register(LinearMover mover, GameObject instance, GameObject prefab)
        {
            if (mover == null || instance == null || prefab == null)
            {
                return;
            }

            _activeMovers.Add(mover);

            if (!_instanceToPrefabMap.ContainsKey(instance))
            {
                _instanceToPrefabMap.Add(instance, prefab);
            }
        }

        public void SetMovementState(bool isMoving)
        {
            foreach (LinearMover mover in _activeMovers)
            {
                if (mover != null && mover.gameObject.activeInHierarchy)
                {
                    mover.SetMoveState(isMoving);
                }
            }
        }

        public void Clear()
        {
            for (int i = _activeMovers.Count - 1; i >= 0; i--)
            {
                LinearMover mover = _activeMovers[i];
                if (mover == null || !mover.gameObject.activeSelf)
                {
                    continue;
                }

                ReturnToPool(mover.gameObject);
            }

            _activeMovers.Clear();
            _instanceToPrefabMap.Clear();
        }

        public void CleanupInactiveMovers()
        {
            for (int i = _activeMovers.Count - 1; i >= 0; i--)
            {
                LinearMover mover = _activeMovers[i];
                if (!mover)
                {
                    _activeMovers.RemoveAt(i);
                    continue;
                }

                if (mover.gameObject.activeInHierarchy)
                {
                    continue;
                }

                _instanceToPrefabMap.Remove(mover.gameObject);
                _activeMovers.RemoveAt(i);
            }
        }

        private void ReturnToPool(GameObject instance)
        {
            if (_instanceToPrefabMap.TryGetValue(instance, out GameObject prefab) && ObjectPool.Instance != null)
            {
                ObjectPool.Instance.Return(prefab, instance);
                return;
            }

            Object.Destroy(instance);
        }
    }
}
