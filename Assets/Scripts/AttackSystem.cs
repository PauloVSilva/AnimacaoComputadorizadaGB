using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackSystem : MonoBehaviour
{
    [SerializeField] private Weapon _currentWeapon;
    [SerializeField] private Weapon[] _weaponList;

    [SerializeField] private SearchSystem _searchSystem;
    [SerializeField] private Enemy _enemy;
    [SerializeField] private float _distanceToTarget;
    [SerializeField] private Enemy _enemyTarget;
    [SerializeField] private bool _canAttack;

    [Header("Search related")]
    [SerializeField] private bool _targetIsWithinAttackRadius;
    [SerializeField] private bool _targetIsWithinSearchRadius;
    [SerializeField] private float _searchRadius;
    [SerializeField] private float _attackRadius;
    [SerializeField] private string _debug;

    private Coroutine _seekNewTarget;
    
    public bool HasTarget => _enemyTarget != null;
    public bool TargetIsWithinAttackRadius => HasTarget && (_distanceToTarget <= _attackRadius);
    public bool TargetIsWithinSearchRadius => HasTarget && (_distanceToTarget <= _searchRadius);
    public Enemy EnemyTarget => _enemyTarget;


    public event System.Action<Enemy> OnTargetChanged;
    public event System.Action OnPerformedAttack;


    private void Start()
    {
        Init();
    }

    private void FixedUpdate()
    {
        EvaluateDistance();
        EvaluateAttackBehaviour();
    }

    private void OnDestroy()
    {
        Finish();
    }

    private void Init()
    {
        _enemy.OnStateChange += AdaptToState;
        _searchSystem.OnTargetFound += SetEnemy;

        _searchSystem.SearchRadius = _searchRadius;
    }

    private void EvaluateDistance()
    {
        if (_enemyTarget == null)
        {
            _debug = "No target";
            return;
        }

        _distanceToTarget = Vector3.Distance(transform.position, _enemyTarget.transform.position);

        if (_distanceToTarget <= _attackRadius)
        {
            _debug = "Target within attack range";
        }
        else if (_distanceToTarget <= _searchRadius)
        {
            _debug = "Target within search range";
        }
        else
        {
            _debug = "Lost sight of target";

            ClearTarget();
        }
    }

    private void EvaluateAttackBehaviour()
    {
        if (!_canAttack) return;

        Attack();
    }

    private void Attack()
    {
        if (!HasTarget) return;
        if (_currentWeapon == null) return;

        if (_currentWeapon.Fire())
        {
            OnPerformedAttack?.Invoke();
        }
    }

    private void Finish()
    {
        _enemy.OnStateChange -= AdaptToState;
        _searchSystem.OnTargetFound -= SetEnemy;

        if (_enemyTarget != null)
        {
            _enemyTarget.OnDied -= ClearTarget;
        }
    }

    public void AdaptToState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Attacking:
                {
                    _canAttack = true;
                }
                break;
            default:
                {
                    _canAttack = false;
                }
                break;
        }
    }

    private void SetEnemy(MonoBehaviour monoBehaviour)
    {
        if (monoBehaviour is Enemy newEnemy)
        {
            if (newEnemy != null && newEnemy.ValidEnemy && newEnemy.Team != _enemy.Team)
            {
                SetEnemyTarget(newEnemy);
            }
        }
    }

    public void SetEnemyTarget(Enemy enemy)
    {
        if (_enemyTarget != null && _enemyTarget == enemy) return;

        if (_enemyTarget != null)
        {
            _enemyTarget.OnDied -= ClearTarget;
        }

        _enemyTarget = enemy;

        if (_enemyTarget != null)
        {
            _enemyTarget.OnDied += ClearTarget;
        }

        OnTargetChanged?.Invoke(_enemyTarget);
    }

    private void ClearTarget()
    {
        SetEnemyTarget(null);
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _searchRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRadius);
    }
}
