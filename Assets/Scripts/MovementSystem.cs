using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro.EditorUtilities;

public class MovementSystem : MonoBehaviour
{
    [SerializeField] private Enemy _enemy;

    [SerializeField] private bool _canMove;
    [SerializeField] private float _speed;
    [SerializeField] private float _walkingSpeed;
    [SerializeField] private float _sprintingSpeed;

    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _targetPosition;
    [SerializeField] private float _distanceToTarget;

    public bool CanMove => _canMove;
    public bool IsWalking => CanMove && Speed <= _walkingSpeed;
    public bool IsSprinting => CanMove && Speed > _walkingSpeed;
    public float Speed => _speed;
    public float WalkingSpeed => _walkingSpeed;
    public float SprintingSpeed => _sprintingSpeed;


    private void Start()
    {
        Init();
    }

    private void FixedUpdate()
    {
        EvaluateMovement();
        Look();
        Move();
    }

    private void OnDestroy()
    {
        Finish();
    }


    private void Init()
    {
        _canMove = true;
        _speed = _walkingSpeed;

        _enemy.OnStateChange += AdaptToState;
    }

    private void EvaluateMovement()
    {
        if (_targetPosition == null)
        {
            PickRandomPosition();
        }

        if (_target != null)
        {
            _targetPosition = _target.position;
        }

        if (_targetPosition != null)
        {
            _distanceToTarget = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), _targetPosition);
        }

        if (CanMove && _distanceToTarget < 0.1f)
        {
            PickRandomPosition();
        }
    }

    private void Look()
    {
        if (_targetPosition == null) return;

        transform.DOLookAt(_targetPosition, 0.1f);
    }

    private void Move()
    {
        if (!_canMove) return;

        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _speed * Time.deltaTime);
        //Rigidbody rigidbody = GetComponent<Rigidbody>();
        //rigidbody.velocity = transform.forward * _speed * Time.deltaTime;
    }

    private void Finish()
    {
        _enemy.OnStateChange -= AdaptToState;
    }



    public void AdaptToState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Roaming:
                {
                    _canMove = true;
                    _speed = _walkingSpeed;

                    SetTarget(null);
                    PickRandomPosition();
                }
                break;
            case EnemyState.Attacking:
                {
                    _canMove = false;
                }
                break;
            case EnemyState.Pursuing:
                {
                    _canMove = true;
                    _speed = _sprintingSpeed;
                }
                break;
            case EnemyState.LookingForHealth:
                {
                    _canMove = true;
                    _speed = _sprintingSpeed;

                    SetTarget(null);
                    PickRandomPosition();
                }
                break;
            case EnemyState.Dying:
                {
                    SetTarget(null);
                    _canMove = false;
                }
                break;
            case EnemyState.Dancing:
                {
                    SetTarget(null);
                    _canMove = false;
                }
                break;
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetTargetPosition(Vector3 target)
    {
        _targetPosition = target;
    }


    private void PickOppositePosition()
    {
        int distance = 10;

        Vector3 newPosition = transform.position - transform.forward * distance;

        SetTargetPosition(newPosition);
    }

    private void PickRandomPosition()
    {
        float x = Random.Range(-25, 25);
        float z = Random.Range(-25, 25);

        SetTargetPosition(new Vector3(x, 0, z));
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_targetPosition, 0.5f);
        Gizmos.DrawLine(transform.position, _targetPosition);
    }
}
