using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;

    [SerializeField] private float _fireRate; //bullets per second

    [SerializeField] private float _lastShotTimeStamp;

    [SerializeField] private Transform _castPoint;

    public bool CanFire => Time.time - _lastShotTimeStamp > 1 / _fireRate;



    public bool Fire()
    {
        if (!CanFire) return false;

        GameObject bullet = Instantiate(_bulletPrefab, _castPoint.transform.position, _castPoint.transform.rotation);

        if (bullet.TryGetComponent(out Projectile projectile))
        {
            projectile.AddVelocityToProjectile(30);
        }

        _lastShotTimeStamp = Time.time;

        return true;
    }
}
