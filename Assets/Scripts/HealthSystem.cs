using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private bool _dead;
    [SerializeField] private int _health;
    [SerializeField, Min(1)] private int _maxHealth = 1;

    [Header("Percentage required to make unit injuried")]
    [SerializeField, Range(0, 100)] private float _injuryThreshhold;

    public bool Dead => _dead;
    public int Health => _health;
    public int MaxHealth => _maxHealth;
    public bool IsInjuried => ((float)_health / (float)_maxHealth) * 100 < _injuryThreshhold;


    public event System.Action OnDied;
    public event System.Action OnDamaged;
    public event System.Action OnHealed;

    private void Start()
    {
        Init();
    }

    private void OnValidate()
    {
        TakeDamage(0);
    }

    private void OnDestroy()
    {
        Finish();
    }


    private void Init()
    {
        _health = _maxHealth;
    }

    private void Finish()
    {

    }


    public void TakeDamage(int damage)
    {
        if (_dead) return;

        _health = Mathf.Max(_health - damage, 0);
        _dead = _health <= 0;

        OnDamaged?.Invoke();

        if (_dead)
        {
            OnDied?.Invoke();
        }
    }

    public void Heal(int heal)
    {
        _health = Mathf.Min(_health + heal, _maxHealth);

        OnHealed?.Invoke();
    }
}
