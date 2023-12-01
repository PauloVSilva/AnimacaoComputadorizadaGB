using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Team _team;
    [SerializeField] private EnemyState _state = EnemyState.None;
    [SerializeField] private EnemyState _previousState = EnemyState.None;

    [SerializeField] private bool _validEnemy;
    [SerializeField] private Animator _animator;

    [Header("Systems")]
    [SerializeField] private SearchSystem _searchSystem;
    [SerializeField] private HealthSystem _healthSystem;
    [SerializeField] private MovementSystem _movementSystem;
    [SerializeField] private AttackSystem _attackSystem;

    public Team Team => _team;
    public EnemyState State => _state;
    public bool ValidEnemy => _validEnemy;

    public event System.Action<EnemyState> OnStateChange;
    public event System.Action OnDied;

    private void Awake()
    {
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    private void Start()
    {
        Init();
    }

    private void FixedUpdate()
    {
        HandleAnimation();
    }

    private void OnDestroy()
    {
        Finish();
    }

    private void Init()
    {
        _healthSystem.OnDamaged += EvaluateBehaviour;
        _healthSystem.OnHealed += EvaluateBehaviour;
        _healthSystem.OnDied += Die;

        _attackSystem.OnTargetChanged += EvaluateBehaviour;
        _attackSystem.OnPerformedAttack += PerformedAttack;

        _searchSystem.OnTargetFound += SetTarget;

        _validEnemy = true;

        ChangeState(EnemyState.Roaming);
        _previousState = _state;
        //this must not be here but I'm too lazy to fix this properly
        _animator.updateMode = AnimatorUpdateMode.Normal;

        StartCoroutine(EvaluateBehaviourInterval());
    }

    private void Finish()
    {
        _healthSystem.OnDamaged -= EvaluateBehaviour;
        _healthSystem.OnHealed -= EvaluateBehaviour;
        _healthSystem.OnDied -= Die;

        _attackSystem.OnTargetChanged -= EvaluateBehaviour;
        _attackSystem.OnPerformedAttack -= PerformedAttack;

        _searchSystem.OnTargetFound -= SetTarget;
    }

    private void HandleAnimation()
    {
        _animator.SetBool("IsWalking", _movementSystem.IsWalking);
        _animator.SetBool("IsRunning", _movementSystem.IsSprinting);
        _animator.SetBool("IsAiming", _state == EnemyState.Attacking);

        if (_state == EnemyState.Dancing) _animator.SetTrigger("Win");
    }

    private void PerformedAttack()
    {
        _animator.ResetTrigger("Shoot");
        _animator.SetTrigger("Shoot");
    }

    private IEnumerator EvaluateBehaviourInterval()
    {
        yield return new WaitForSeconds(0.1f);

        EvaluateBehaviour();

        StartCoroutine(EvaluateBehaviourInterval());
    }

    private void SetTarget(MonoBehaviour monoBehaviour)
    {
        if (monoBehaviour == null) return;

        Enemy enemy = monoBehaviour as Enemy;

        if (enemy != null && enemy.Team == _team)
        {
            return;
        }

        _movementSystem.SetTarget(monoBehaviour.transform);
    }

    private void EvaluateBehaviour()
    {
        if (_healthSystem.IsInjuried && !_attackSystem.HasTarget)
        {
            ChangeState(EnemyState.LookingForHealth);
        }

        if (_healthSystem.IsInjuried && _attackSystem.HasTarget)
        {
            int randomChance = Random.Range(0, 100);

            if (randomChance < 20)
            {
                ChangeState(EnemyState.LookingForHealth);
            }
        }
        else if (!_healthSystem.IsInjuried && !_attackSystem.HasTarget)
        {
            ChangeState(EnemyState.Roaming);
        }
        else if (!_healthSystem.IsInjuried && _attackSystem.HasTarget)
        {
            _movementSystem.SetTarget(_attackSystem.EnemyTarget.transform);

            if (_attackSystem.TargetIsWithinAttackRadius)
            {
                ChangeState(EnemyState.Attacking);
            }
            else if (_attackSystem.TargetIsWithinSearchRadius)
            {
                ChangeState(EnemyState.Pursuing);
            }
        }
    }

    private void EvaluateBehaviour(Enemy enemy)
    {
        /*
        if (enemy == null)
        {
            ChangeState(EnemyState.Roaming);
        }
        else
        {
            _movementSystem.SetTarget(enemy.transform);
        }
        */
    }

    public void ChangeState(EnemyState state)
    {
        if (State == EnemyState.Dying) return;
        if (State == EnemyState.Dancing) return;

        if (_state != state)
        {
            _previousState = _state;
            _state = state;

            OnStateChange?.Invoke(state);
        }

        if (State == EnemyState.Roaming)
        {
            //_searchSystem.LookForTarget<Enemy>();

            _searchSystem.InitiateSearch<Enemy>(int.MaxValue, 0.1f);
            
        }

        if (State == EnemyState.LookingForHealth)
        {
            //_searchSystem.LookForTarget<MedKit>();

            _searchSystem.InitiateSearch<MedKit>(20, 0.5f);

        }
    }

    private void ReturnToPreviousState()
    {
        ChangeState(_previousState);
    }

    private void Die()
    {
        if (!_validEnemy) return;

        _validEnemy = false;

        OnDied?.Invoke();

        ChangeState(EnemyState.Dying);

        _animator.SetTrigger("Die");

        Destroy(gameObject, 2f);
    }

    public void Win()
    {
        if (!_validEnemy) return;

        ChangeState(EnemyState.Dancing);

        _animator.SetTrigger("Win");
    }


    /*
    public T LookForTarget<T>() where T : MonoBehaviour
    {
        T newTarget = FindTarget<T>();

        if (newTarget != null)
        {
            _movementSystem.SetTarget(newTarget.transform);

            //if (typeof(T) == typeof(Enemy))
            //{
            //    Enemy newEnemy = newTarget as Enemy;
            //
            //    if (newEnemy != null && newEnemy.ValidEnemy)
            //    {
            //        ChangeState(EnemyState.Attacking);
            //
            //        _attackSystem.SetEnemyTarget(newTarget as Enemy);
            //    }
            //}
        }

        OnTargetFound?.Invoke(newTarget);

        return newTarget;
    }

    public IEnumerator LookForTargetInterval<T>(int attempts, float delay) where T : MonoBehaviour
    {
        yield return new WaitForSeconds(delay);

        if (!LookForTarget<T>())
        {
            attempts--;

            if (attempts > 0)
            {
                _seekNewTarget = StartCoroutine(LookForTargetInterval<T>(attempts, delay));
            }
            else
            {
                if (_seekNewTarget != null)
                {
                    StopCoroutine(_seekNewTarget);
                }

                _seekNewTarget = null;

                //ReturnToPreviousState();

                ChangeState(EnemyState.Roaming);
            }
        }
        else
        {
            if (_seekNewTarget != null)
            {
                StopCoroutine(_seekNewTarget);
            }

            _seekNewTarget = null;
        }
    }

    public T FindTarget<T>() where T : MonoBehaviour
    {
        List<T> targets = FindObjectsOfType<T>().ToList();
        List<T> otherTargets = targets.Where(target => target != this).ToList();

        if (otherTargets.Count > 0)
        {
            GameObject nearestObject = FindNearestTarget(otherTargets.ToList());

            if (nearestObject != null)
            {
                return nearestObject.GetComponent<T>();
            }
        }

        return null;
    }

    public GameObject FindNearestTarget<T>(List<T> list) where T : MonoBehaviour
    {
        GameObject nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (var item in list)
        {
            float distance = Vector3.Distance(transform.position, item.transform.position);

            if (distance < nearestDistance && distance <= 5)
            {
                nearest = item.gameObject;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
    */
}

public enum Team
{
    None = 0,
    Red = 1,
    Blue = 2,
}

public enum EnemyState
{
    None = -1,
    Roaming = 0,
    Attacking = 1,
    Pursuing = 7,
    SwappingGuns = 2,
    LookingForHealth = 3,
    Airborne = 4,
    Dying = 5,
    Dancing = 6,
}