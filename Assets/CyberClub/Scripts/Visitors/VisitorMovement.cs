using System;
using UnityEngine;
using UnityEngine.AI;

public class VisitorMovement : MonoBehaviour
{
    private const string MovementParameter = "IsMovement";

    private Animator _animator;
    private NavMeshAgent _agent;

    private Action _onComplete;
    private Vector3 _targetPos;
    private bool _hasTarget;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!_hasTarget)
            return;

        OnCompleteMove();
    }

    public void Move(Vector3 target, Action onComplete = null)
    {
        if (_agent == null || !_agent.enabled)
        {
            Debug.LogError("VisitorMovement: у посетителя нет активного NavMeshAgent.");
            return;
        }

        _targetPos = target;
        _onComplete = onComplete;

        _agent.updatePosition = true;
        _agent.updateRotation = true;
        _agent.isStopped = false;
        _agent.SetDestination(_targetPos);

        _hasTarget = true;
        IsMoving = true;

        SetMovementAnimation(true);
    }

    public void StopMovement()
    {
        _hasTarget = false;
        IsMoving = false;
        _onComplete = null;

        if (_agent != null && _agent.enabled)
        {
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
        }

        SetMovementAnimation(false);
    }

    private void OnCompleteMove()
    {
        if (_agent == null || _agent.pathPending)
            return;

        bool isReached =
            _agent.remainingDistance <= _agent.stoppingDistance &&
            (!_agent.hasPath || _agent.velocity.sqrMagnitude < 0.01f);

        if (!isReached)
            return;

        IsMoving = false;
        _hasTarget = false;

        SetMovementAnimation(false);
        RotateToTarget();

        Action complete = _onComplete;
        _onComplete = null;

        complete?.Invoke();
    }

    private void RotateToTarget()
    {
        Vector3 direction = _targetPos - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private void SetMovementAnimation(bool value)
    {
        if (_animator != null)
            _animator.SetBool(MovementParameter, value);
    }
}