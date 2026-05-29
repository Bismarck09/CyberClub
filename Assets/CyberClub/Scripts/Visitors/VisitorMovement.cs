using UnityEngine;
using UnityEngine.AI;
using System;

public class VisitorMovement : MonoBehaviour
{
    private Animator _animator;
     
    private NavMeshAgent _agent;
    private Action _onComplete;
    private Vector3 _targetPos;
    private bool _hasTarget;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!_hasTarget)
            return;

        OnCompleteMove();
    }

    private void OnCompleteMove()
    {
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance && (!_agent.hasPath || _agent.velocity.sqrMagnitude < 0.01f))
        {
            IsMoving = false;
            _hasTarget = false;
            _animator.SetBool("IsMovement", false);
            
            RotateToTarget();
            _onComplete?.Invoke();
        }
        else
            IsMoving = true;
    }

    public void Move(Vector3 target, Action onComplete = null)
    {
        _targetPos = target;
        _onComplete = onComplete;

        _agent.SetDestination(_targetPos);
        _hasTarget = true;
        _animator.SetBool("IsMovement", true);
    }

    private void RotateToTarget()
    {
        Vector3 direction = (_targetPos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
