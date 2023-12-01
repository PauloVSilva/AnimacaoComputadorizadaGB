using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;

    [SerializeField] private Enemy _caster;

    [SerializeField] private Weapon _weaponOfCast;

    [SerializeField] private int _damagePower;

    private void Start()
    {
        Init();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthSystem healthSystem))
        {
            healthSystem.TakeDamage(_damagePower);

            Destroy(gameObject);
        }
    }


    private void Init()
    {
        Destroy(gameObject, 5);
    }

    public void SetDamagePower(int power)
    {
        _damagePower = power;
    }

    public void AddVelocityToProjectile(float velocity)
    {
        _rigidbody.AddForce(transform.forward * velocity, ForceMode.Impulse);
    }
}
