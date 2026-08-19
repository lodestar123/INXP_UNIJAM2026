using System.Collections.Generic;
using UnityEngine;

namespace Galaga
{
    /// <summary>
    /// 플레이어 레이저/적 총알 프리팹 풀
    /// </summary>
    public class GalagaProjectilePool : MonoBehaviour
    {
        private GameObject _laserPrefab;
        private GameObject _bulletPrefab;
        private Transform _activeParent;
        private Transform _laserStock;
        private Transform _bulletStock;

        private readonly List<GalagaLaser> _laserStockList = new List<GalagaLaser>();
        private readonly List<GalagaEnemyBullet> _bulletStockList = new List<GalagaEnemyBullet>();
        private readonly List<GalagaLaser> _activeLasers = new List<GalagaLaser>();
        private readonly List<GalagaEnemyBullet> _activeBullets = new List<GalagaEnemyBullet>();

        public void Initialize(GalagaConfig config, Transform activeParent)
        {
            _laserPrefab = config != null ? config.playerLaserPrefab : null;
            _bulletPrefab = config != null ? config.enemyBulletPrefab : null;
            _activeParent = activeParent;

            _laserStock = CreateStockRoot("LaserStock");
            _bulletStock = CreateStockRoot("BulletStock");
        }

        private Transform CreateStockRoot(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.SetActive(false);
            return go.transform;
        }

        public void SpawnLaser(Vector3 position, float speed, int damage, float topBound, Sprite sprite)
        {
            if (_laserPrefab == null)
            {
                Debug.LogWarning("[Galaga] GalagaConfig.playerLaserPrefab이 비어 있어 레이저를 발사하지 않습니다.");
                return;
            }

            GalagaLaser laser = GetLaser();
            if (laser == null) return;

            Transform t = laser.transform;
            t.SetParent(_activeParent, false);
            t.SetPositionAndRotation(position, Quaternion.identity);

            ApplySprite(laser.gameObject, sprite);
            laser.Initialize(speed, damage, topBound, this);
            _activeLasers.Add(laser);
            laser.gameObject.SetActive(true);
        }

        public void SpawnBullet(Vector2 origin, Vector2 direction, float speed, float despawnY, Sprite sprite)
        {
            if (_bulletPrefab == null)
            {
                Debug.LogWarning("[Galaga] GalagaConfig.enemyBulletPrefab이 비어 있어 총알을 발사하지 않습니다.");
                return;
            }

            GalagaEnemyBullet bullet = GetBullet();
            if (bullet == null) return;

            Transform t = bullet.transform;
            t.SetParent(_activeParent, false);
            t.position = origin;

            ApplySprite(bullet.gameObject, sprite);
            bullet.Initialize(direction, speed, despawnY, this);
            _activeBullets.Add(bullet);
            bullet.gameObject.SetActive(true);
        }

        public void ReturnLaser(GalagaLaser laser)
        {
            if (laser == null) return;
            if (!_activeLasers.Remove(laser)) return;

            laser.gameObject.SetActive(false);
            laser.transform.SetParent(_laserStock, false);
            _laserStockList.Add(laser);
        }

        public void ReturnBullet(GalagaEnemyBullet bullet)
        {
            if (bullet == null) return;
            if (!_activeBullets.Remove(bullet)) return;

            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(_bulletStock, false);
            _bulletStockList.Add(bullet);
        }

        // 전환/사망 시 화면의 레이저/탄을 모두 회수
        public void CollectAll()
        {
            while (_activeLasers.Count > 0)
            {
                ReturnLaser(_activeLasers[_activeLasers.Count - 1]);
            }

            while (_activeBullets.Count > 0)
            {
                ReturnBullet(_activeBullets[_activeBullets.Count - 1]);
            }
        }

        private GalagaLaser GetLaser()
        {
            if (_laserStockList.Count > 0)
            {
                int last = _laserStockList.Count - 1;
                GalagaLaser laser = _laserStockList[last];
                _laserStockList.RemoveAt(last);
                return laser;
            }

            GameObject go = Instantiate(_laserPrefab, _laserStock);
            go.name = "PlayerLaser";
            go.SetActive(false);
            return go.GetComponent<GalagaLaser>();
        }

        private GalagaEnemyBullet GetBullet()
        {
            if (_bulletStockList.Count > 0)
            {
                int last = _bulletStockList.Count - 1;
                GalagaEnemyBullet bullet = _bulletStockList[last];
                _bulletStockList.RemoveAt(last);
                return bullet;
            }

            GameObject go = Instantiate(_bulletPrefab, _bulletStock);
            go.name = "EnemyBullet";
            go.SetActive(false);
            return go.GetComponent<GalagaEnemyBullet>();
        }

        private static void ApplySprite(GameObject go, Sprite sprite)
        {
            if (sprite == null) return;

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) return;
            sr.sprite = sprite;
            sr.color = Color.white;
        }
    }
}
